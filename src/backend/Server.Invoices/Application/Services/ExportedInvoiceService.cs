using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Application.Services;

public sealed class ExportedInvoiceService(
    IExportedInvoiceRepository repository,
    TimeProvider timeProvider) : IExportedInvoiceService
{
    public async Task<GeneratedInvoicePageDto> GetPageAsync(
        GeneratedInvoiceListQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await repository.QueryGeneratedInvoicesAsync(
            new GeneratedInvoiceQuery(
                query.DateAfter,
                query.DateBefore,
                query.ClientId,
                CleanOptional(query.Status),
                CleanOptional(query.Search),
                (query.Page - 1) * query.PageSize,
                query.PageSize),
            cancellationToken);
        var pages = result.TotalItems == 0
            ? 0
            : (int)Math.Ceiling((double)result.TotalItems / query.PageSize);
        return new GeneratedInvoicePageDto(
            result.Items.Select(Map).ToList(),
            query.Page,
            query.PageSize,
            result.TotalItems,
            pages);
    }

    public async Task<GenerateInvoiceOptionsDto> GetOptionsAsync(
        CancellationToken cancellationToken)
    {
        var clients = await repository.GetClientsWithEligibleWorkAsync(cancellationToken);
        var projects = await repository.GetProjectsWithEligibleWorkAsync(cancellationToken);
        return new GenerateInvoiceOptionsDto(
            clients.Select(client => new ExportOptionDto(client.Id, client.Name)).ToList(),
            projects.Select(project => new ExportOptionDto(project.Id, project.Name)).ToList());
    }

    public async Task<GeneratedInvoiceDto> GenerateAsync(
        GenerateInvoiceDto request,
        CancellationToken cancellationToken)
    {
        var business = await repository.GetBusinessProfileAsync(cancellationToken)
            ?? throw new InvoiceExportException(
                "businessProfile",
                "Configure your business profile before generating an invoice.");
        var client = await repository.GetClientAsync(request.ClientId, cancellationToken)
            ?? throw new InvoiceExportException("clientId", "Select a configured customer.");
        if (request.ProjectId is not null &&
            await repository.GetProjectAsync(request.ProjectId.Value, cancellationToken) is null)
            throw new InvoiceExportException("projectId", "Select a configured project.");

        var currency = request.Currency.Trim().ToUpperInvariant();
        var workRecords = await repository.GetEligibleWorkRecordsAsync(
            request.ClientId,
            request.ProjectId,
            currency,
            cancellationToken);
        if (workRecords.Count == 0)
            throw new InvoiceExportException(
                "workRecords",
                "No unexported work records match these options.");

        var configuration = await repository.GetInvoiceConfigurationAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var id = Guid.NewGuid();
        var invoice = new GeneratedInvoice
        {
            Id = id,
            InvoiceNumber = $"INV-{request.IssueDate:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            ClientId = client.Id,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Currency = currency,
            Subtotal = workRecords.Sum(record => record.Value),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = GeneratedInvoiceStatuses.Exported,
            InvoiceStyle = configuration?.Style ?? InvoiceStyles.Basic,
            LogoData = configuration?.LogoData?.ToArray(),
            BusinessName = string.IsNullOrWhiteSpace(business.TradingName)
                ? business.LegalName
                : business.TradingName,
            BusinessEmail = business.Email,
            BusinessPhone = business.Phone,
            BusinessAddress = Address(
                business.AddressLine1,
                business.AddressLine2,
                business.City,
                business.County,
                business.Postcode),
            CompanyNumber = business.CompanyNumber,
            VatNumber = business.VatNumber,
            ClientName = client.Name,
            ClientEmail = client.Email,
            ClientPhone = client.Phone,
            ClientAddress = Address(
                client.AddressLine1,
                client.AddressLine2,
                client.City,
                client.County,
                client.Postcode),
            WorkRecords = workRecords.ToList(),
            CreatedAt = now,
            UpdatedAt = now
        };
        foreach (var record in workRecords)
        {
            record.GeneratedInvoiceId = invoice.Id;
            record.GeneratedInvoice = invoice;
            record.Status = WorkRecordStatuses.Invoiced;
            record.UpdatedAt = now;
        }

        await repository.AddGeneratedInvoiceAsync(invoice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }

    public async Task<GeneratedInvoiceDto?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetGeneratedInvoiceAsync(id, false, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<GeneratedInvoiceDto?> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken)
    {
        if (!GeneratedInvoiceStatuses.All.Contains(status))
            throw new InvoiceExportException("status", "Status is not supported.");
        var invoice = await repository.GetGeneratedInvoiceAsync(id, true, cancellationToken);
        if (invoice is null) return null;
        if (invoice.Status == GeneratedInvoiceStatuses.Deleted && status != GeneratedInvoiceStatuses.Deleted)
            throw new InvoiceExportException("status", "A deleted invoice cannot be restored.");
        var now = timeProvider.GetUtcNow();
        invoice.Status = status;
        invoice.UpdatedAt = now;
        if (status == GeneratedInvoiceStatuses.Deleted)
        {
            foreach (var record in invoice.WorkRecords.ToList())
            {
                record.GeneratedInvoiceId = null;
                record.GeneratedInvoice = null;
                record.Status = WorkRecordStatuses.Uninvoiced;
                record.UpdatedAt = now;
            }

            invoice.WorkRecords.Clear();
        }
        await repository.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }

    public Task<GeneratedInvoice?> GetDocumentSourceAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        repository.GetGeneratedInvoiceAsync(id, false, cancellationToken);

    private static GeneratedInvoiceDto Map(GeneratedInvoice invoice) => new(
        invoice.Id,
        invoice.InvoiceNumber,
        invoice.ClientId,
        invoice.ClientName,
        invoice.IssueDate,
        invoice.DueDate,
        invoice.Currency,
        invoice.Subtotal,
        invoice.Notes,
        invoice.Status,
        invoice.WorkRecords
            .OrderBy(record => record.CreatedAt)
            .Select(record => new GeneratedInvoiceLineDto(
                record.Id,
                record.Description,
                record.Project.Name,
                record.Value))
            .ToList(),
        invoice.CreatedAt);

    private static string Address(params string[] parts) =>
        string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static string? CleanOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class InvoiceExportException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
