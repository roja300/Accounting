using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;
using Server.Invoices.Domain;

namespace Server.Invoices.Tests;

public class ExportedInvoiceServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 19, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldGenerateInvoiceAndMarkWorkRecordsExported_WhenOptionsMatch()
    {
        var client = CreateClient();
        var project = CreateProject();
        var first = CreateWorkRecord(client, project, 125m);
        var second = CreateWorkRecord(client, project, 75m);
        var repository = new MockExportedInvoiceRepository
        {
            BusinessProfile = CreateBusinessProfile(),
            InvoiceConfiguration = new InvoiceConfiguration
            {
                Style = InvoiceStyles.Modern,
                LogoData = [1, 2, 3]
            },
            Client = client,
            Project = project,
            EligibleWorkRecords = [first, second]
        };
        var service = new ExportedInvoiceService(repository, new FixedTimeProvider(Now));

        var result = await service.GenerateAsync(
            new GenerateInvoiceDto(
                client.Id,
                project.Id,
                "gbp",
                new DateOnly(2026, 10, 19),
                new DateOnly(2026, 11, 18),
                "Payment due within 30 days."),
            CancellationToken.None);

        var saved = Assert.IsType<GeneratedInvoice>(repository.AddedGeneratedInvoice);
        Assert.StartsWith("INV-20261019-", saved.InvoiceNumber, StringComparison.Ordinal);
        Assert.Equal(200m, saved.Subtotal);
        Assert.Equal("GBP", saved.Currency);
        Assert.Equal(GeneratedInvoiceStatuses.Exported, saved.Status);
        Assert.Equal(InvoiceStyles.Modern, saved.InvoiceStyle);
        Assert.Equal([1, 2, 3], saved.LogoData);
        Assert.Equal("Example Studio", saved.BusinessName);
        Assert.Equal("Acme Ltd", saved.ClientName);
        Assert.All([first, second], record => Assert.Equal(saved.Id, record.GeneratedInvoiceId));
        Assert.All([first, second], record =>
            Assert.Equal(WorkRecordStatuses.Invoiced, record.Status));
        Assert.Equal(2, result.Lines.Count);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldRejectExport_WhenBusinessProfileIsMissing()
    {
        var repository = new MockExportedInvoiceRepository();
        var service = new ExportedInvoiceService(repository, new FixedTimeProvider(Now));

        var exception = await Assert.ThrowsAsync<InvoiceExportException>(() =>
            service.GenerateAsync(
                new GenerateInvoiceDto(
                    Guid.NewGuid(),
                    null,
                    "GBP",
                    new DateOnly(2026, 10, 19),
                    new DateOnly(2026, 11, 18),
                    null),
                CancellationToken.None));

        Assert.Equal("businessProfile", exception.Field);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldMarkInvoicePaid_WhenInvoiceExists()
    {
        var invoice = CreateGeneratedInvoice();
        var repository = new MockExportedInvoiceRepository { StoredGeneratedInvoice = invoice };
        var service = new ExportedInvoiceService(repository, new FixedTimeProvider(Now));

        var result = await service.UpdateStatusAsync(
            invoice.Id,
            GeneratedInvoiceStatuses.PaymentReceived,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(GeneratedInvoiceStatuses.PaymentReceived, invoice.Status);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldHideInvoiceAndRestoreWorkRecords_WhenInvoiceIsDeleted()
    {
        var invoice = CreateGeneratedInvoice();
        foreach (var record in invoice.WorkRecords)
        {
            record.GeneratedInvoiceId = invoice.Id;
            record.GeneratedInvoice = invoice;
            record.Status = WorkRecordStatuses.Invoiced;
        }
        var releasedRecords = invoice.WorkRecords.ToList();
        var repository = new MockExportedInvoiceRepository { StoredGeneratedInvoice = invoice };
        var service = new ExportedInvoiceService(repository, new FixedTimeProvider(Now));

        var result = await service.UpdateStatusAsync(
            invoice.Id,
            GeneratedInvoiceStatuses.Deleted,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(GeneratedInvoiceStatuses.Deleted, invoice.Status);
        Assert.Empty(invoice.WorkRecords);
        Assert.All(releasedRecords, record =>
        {
            Assert.Null(record.GeneratedInvoiceId);
            Assert.Null(record.GeneratedInvoice);
            Assert.Equal(WorkRecordStatuses.Uninvoiced, record.Status);
            Assert.Equal(Now, record.UpdatedAt);
        });
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Theory]
    [InlineData(InvoiceStyles.Basic)]
    [InlineData(InvoiceStyles.Modern)]
    [InlineData(InvoiceStyles.Minimal)]
    public void ShouldGeneratePdfDocument_WhenInvoiceStyleIsConfigured(string style)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var document = new InvoiceDocumentService();
        var invoice = CreateGeneratedInvoice();
        invoice.InvoiceStyle = style;

        var bytes = document.Generate(invoice);

        Assert.True(bytes.Length > 1_000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    private static BusinessProfile CreateBusinessProfile() => new()
    {
        LegalName = "Example Freelancer",
        TradingName = "Example Studio",
        Email = "owner@example.test",
        AddressLine1 = "1 High Street",
        City = "London",
        Postcode = "SW1A 1AA"
    };

    private static Client CreateClient() => new()
    {
        Id = Guid.Parse("b31f5928-64b0-4dd7-aad1-3a3ca55158ed"),
        Name = "Acme Ltd",
        Email = "accounts@acme.test",
        AddressLine1 = "2 Market Road",
        City = "Manchester",
        Postcode = "M1 1AA"
    };

    private static Project CreateProject() => new()
    {
        Id = Guid.Parse("dd0c3ad8-e41c-47f4-9c15-bfb0c5a132b4"),
        Name = "Retainer"
    };

    private static Invoice CreateWorkRecord(Client client, Project project, decimal value) => new()
    {
        Id = Guid.NewGuid(),
        Description = "Monthly bookkeeping",
        Value = value,
        Currency = "GBP",
        ClientId = client.Id,
        Client = client,
        ProjectId = project.Id,
        Project = project,
        CreatedAt = Now,
        UpdatedAt = Now
    };

    private static GeneratedInvoice CreateGeneratedInvoice()
    {
        var client = CreateClient();
        var project = CreateProject();
        return new GeneratedInvoice
        {
            Id = Guid.Parse("ed652b89-8923-4c68-920c-2e32bbb4150f"),
            InvoiceNumber = "INV-20261019-ABC123",
            ClientId = client.Id,
            ClientName = client.Name,
            ClientEmail = client.Email,
            ClientAddress = "2 Market Road, Manchester, M1 1AA",
            BusinessName = "Example Studio",
            BusinessEmail = "owner@example.test",
            BusinessAddress = "1 High Street, London, SW1A 1AA",
            IssueDate = new DateOnly(2026, 10, 19),
            DueDate = new DateOnly(2026, 11, 18),
            Currency = "GBP",
            Subtotal = 125m,
            Status = GeneratedInvoiceStatuses.Exported,
            WorkRecords = [CreateWorkRecord(client, project, 125m)],
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }
}
