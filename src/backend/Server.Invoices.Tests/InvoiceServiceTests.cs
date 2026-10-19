using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;
using Server.Invoices.Domain;

namespace Server.Invoices.Tests;

public class InvoiceServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldReturnFilteredPage_WhenQueryIsProvided()
    {
        var invoice = CreateInvoiceEntity();
        var context = new MockInvoiceContext { Invoices = [invoice] };
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var query = new InvoiceListQueryDto
        {
            DateAfter = new DateOnly(2026, 10, 1),
            DateBefore = new DateOnly(2026, 10, 18),
            Client = " acme ltd ",
            Project = " retainer ",
            Search = " monthly ",
            Page = 2,
            PageSize = 6
        };

        var page = await service.GetPageAsync(query, CancellationToken.None);

        var persistenceQuery = Assert.IsType<Infrastructure.Abstractions.InvoiceQuery>(
            context.CapturedQuery);
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), persistenceQuery.CreatedOnOrAfter);
        Assert.Equal(
            new DateTimeOffset(new DateTime(2026, 10, 18, 23, 59, 59, 999), TimeSpan.Zero).AddTicks(9_999),
            persistenceQuery.CreatedOnOrBefore);
        Assert.Equal("ACME LTD", persistenceQuery.NormalizedClient);
        Assert.Equal("RETAINER", persistenceQuery.NormalizedProject);
        Assert.Equal("monthly", persistenceQuery.DescriptionSearch);
        Assert.Equal(6, persistenceQuery.Offset);
        Assert.Equal(6, persistenceQuery.Limit);
        Assert.Equal(2, page.Page);
        Assert.Equal(6, page.PageSize);
        Assert.Equal(1, page.TotalItems);
        Assert.Equal(1, page.TotalPages);
        var item = Assert.Single(page.Items);
        Assert.Equal(invoice.Id, item.Id);
        Assert.Equal(invoice.Client.Name, item.Client);
        Assert.Equal(invoice.Project.Name, item.Project);
    }

    [Fact]
    public async Task ShouldPersistInvoiceWithNewClientAndProject_WhenNamesDoNotExist()
    {
        var context = new MockInvoiceContext();
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice();

        var response = await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal("Monthly bookkeeping", saved.Description);
        Assert.Equal(125.50m, saved.Value);
        Assert.Equal("GBP", saved.Currency);
        Assert.Equal("Completed October records.", saved.Comments);
        Assert.Equal("Acme Ltd", saved.Client.Name);
        Assert.Equal("ACME LTD", saved.Client.NormalizedName);
        Assert.Equal("Retainer", saved.Project.Name);
        Assert.Equal("RETAINER", saved.Project.NormalizedName);
        Assert.Equal(Now, saved.CreatedAt);
        Assert.Equal(Now, saved.UpdatedAt);
        Assert.Equal(1, context.SaveChangesCalls);
        Assert.Equal(saved.Id, response.Id);
    }

    [Fact]
    public async Task ShouldReuseClientAndProject_WhenNormalizedNamesExist()
    {
        var existingClient = CreateClient();
        var existingProject = CreateProject();
        var context = new MockInvoiceContext
        {
            Client = existingClient,
            Project = existingProject
        };
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice() with
        {
            Client = " acme ltd ",
            Project = " retainer "
        };

        await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.Same(existingClient, saved.Client);
        Assert.Same(existingProject, saved.Project);
        Assert.Equal(existingClient.Id, saved.ClientId);
        Assert.Equal(existingProject.Id, saved.ProjectId);
    }

    [Fact]
    public async Task ShouldPersistEmptyComments_WhenCommentsAreOmitted()
    {
        var context = new MockInvoiceContext();
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = CreateInvoice() with { Comments = null };

        var response = await service.CreateAsync(request, CancellationToken.None);

        var saved = Assert.IsType<Invoice>(context.AddedInvoice);
        Assert.Equal(string.Empty, saved.Comments);
        Assert.Equal(string.Empty, response.Comments);
    }

    [Fact]
    public async Task ShouldUpdateInvoiceAndRelationships_WhenInvoiceExists()
    {
        var invoice = CreateInvoiceEntity();
        var context = new MockInvoiceContext { StoredInvoice = invoice };
        var service = new InvoiceService(context, new FixedTimeProvider(Now));
        var request = new InvoiceDto(
            " Year-end accounts ",
            950m,
            "eur",
            " Draft prepared. ",
            " Northwind Ltd ",
            " Annual accounts ");

        var response = await service.UpdateAsync(
            invoice.Id,
            request,
            CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("Year-end accounts", invoice.Description);
        Assert.Equal(950m, invoice.Value);
        Assert.Equal("EUR", invoice.Currency);
        Assert.Equal("Draft prepared.", invoice.Comments);
        Assert.Equal("Northwind Ltd", invoice.Client.Name);
        Assert.Equal("NORTHWIND LTD", invoice.Client.NormalizedName);
        Assert.Equal("Annual accounts", invoice.Project.Name);
        Assert.Equal("ANNUAL ACCOUNTS", invoice.Project.NormalizedName);
        Assert.Equal(Now, invoice.UpdatedAt);
        Assert.Equal(1, context.SaveChangesCalls);
        Assert.Equal(invoice.Id, response.Id);
    }

    [Fact]
    public async Task ShouldReturnNullWithoutSaving_WhenUpdatedInvoiceDoesNotExist()
    {
        var context = new MockInvoiceContext();
        var service = new InvoiceService(context, new FixedTimeProvider(Now));

        var response = await service.UpdateAsync(
            Guid.NewGuid(),
            CreateInvoice(),
            CancellationToken.None);

        Assert.Null(response);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    private static InvoiceDto CreateInvoice()
    {
        return new InvoiceDto(
            " Monthly bookkeeping ",
            125.50m,
            "gbp",
            " Completed October records. ",
            " Acme Ltd ",
            " Retainer ");
    }

    private static Client CreateClient()
    {
        return new Client
        {
            Id = Guid.Parse("0af19caf-dd92-4ecb-acfe-67e22e2e7d0a"),
            Name = "Acme Ltd",
            NormalizedName = "ACME LTD",
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }

    private static Project CreateProject()
    {
        return new Project
        {
            Id = Guid.Parse("d843fe1f-e8b7-45a2-8b88-361bfc977fb7"),
            Name = "Retainer",
            NormalizedName = "RETAINER",
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }

    private static Invoice CreateInvoiceEntity()
    {
        var client = CreateClient();
        var project = CreateProject();
        return new Invoice
        {
            Id = Guid.Parse("1f184969-9587-4103-b6ee-80a467669fa8"),
            Description = "Monthly bookkeeping",
            Value = 125.50m,
            Currency = "GBP",
            Comments = "Completed October records.",
            ClientId = client.Id,
            ProjectId = project.Id,
            Client = client,
            Project = project,
            CreatedAt = Now,
            UpdatedAt = Now
        };
    }
}
