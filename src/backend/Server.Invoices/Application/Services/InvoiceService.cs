using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Application.Services;

public sealed class InvoiceService(
    IInvoiceContext context,
    TimeProvider timeProvider) : IInvoiceService
{
    public async Task<InvoicePageDto> GetPageAsync(
        InvoiceListQueryDto query,
        CancellationToken cancellationToken)
    {
        var persistenceQuery = new InvoiceQuery(
            ToStartOfDay(query.DateAfter),
            ToEndOfDay(query.DateBefore),
            NormalizeOptionalName(query.Client),
            NormalizeOptionalName(query.Project),
            NormalizeOptionalText(query.Search),
            (long)(query.Page - 1) * query.PageSize,
            query.PageSize);
        var result = await context.QueryInvoicesAsync(persistenceQuery, cancellationToken);
        var totalPages = result.TotalItems == 0
            ? 0
            : (int)Math.Ceiling((double)result.TotalItems / query.PageSize);

        return new InvoicePageDto(
            result.Items.Select(Map).ToList(),
            query.Page,
            query.PageSize,
            result.TotalItems,
            totalPages);
    }

    public async Task<InvoiceResponseDto> CreateAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var clientName = invoice.Client.Trim();
        var projectName = invoice.Project.Trim();
        var client = await context.FindClientAsync(NormalizeName(clientName), cancellationToken)
            ?? CreateClient(clientName, now);
        var project = await context.FindProjectAsync(NormalizeName(projectName), cancellationToken)
            ?? CreateProject(projectName, now);

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            Description = invoice.Description.Trim(),
            Value = invoice.Value,
            Currency = invoice.Currency.Trim().ToUpperInvariant(),
            Comments = invoice.Comments?.Trim() ?? string.Empty,
            ClientId = client.Id,
            ProjectId = project.Id,
            Client = client,
            Project = project,
            CreatedAt = now,
            UpdatedAt = now
        };

        await context.AddInvoiceAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<InvoiceResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await context.GetInvoiceAsync(id, cancellationToken);
        return invoice is null ? null : Map(invoice);
    }

    public async Task<InvoiceOptionsDto> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var clients = await context.GetClientNamesAsync(cancellationToken);
        var projects = await context.GetProjectNamesAsync(cancellationToken);
        return new InvoiceOptionsDto(clients, projects);
    }

    private static Client CreateClient(string name, DateTimeOffset now)
    {
        return new Client
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = NormalizeName(name),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Project CreateProject(string name, DateTimeOffset now)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = NormalizeName(name),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static string? NormalizeOptionalName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : NormalizeName(name);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTimeOffset? ToStartOfDay(DateOnly? date)
    {
        return date is null
            ? null
            : new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    private static DateTimeOffset? ToEndOfDay(DateOnly? date)
    {
        return date is null
            ? null
            : new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
    }

    private static InvoiceResponseDto Map(Invoice invoice)
    {
        return new InvoiceResponseDto(
            invoice.Id,
            invoice.Description,
            invoice.Value,
            invoice.Currency,
            invoice.Comments,
            invoice.Client.Name,
            invoice.Project.Name,
            invoice.CreatedAt);
    }
}
