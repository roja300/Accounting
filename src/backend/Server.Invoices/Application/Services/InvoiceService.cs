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
        var client = await GetClientAsync(clientName, cancellationToken);
        var project = await GetOrCreateProjectAsync(projectName, now, cancellationToken);

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

    public async Task<InvoiceResponseDto?> UpdateAsync(
        Guid id,
        InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        var entity = await context.GetInvoiceForUpdateAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        var clientName = invoice.Client.Trim();
        var projectName = invoice.Project.Trim();
        var client = await GetClientAsync(clientName, cancellationToken);
        var project = await GetOrCreateProjectAsync(projectName, now, cancellationToken);

        entity.Description = invoice.Description.Trim();
        entity.Value = invoice.Value;
        entity.Currency = invoice.Currency.Trim().ToUpperInvariant();
        entity.Comments = invoice.Comments?.Trim() ?? string.Empty;
        entity.ClientId = client.Id;
        entity.ProjectId = project.Id;
        entity.Client = client;
        entity.Project = project;
        entity.UpdatedAt = now;

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

    public async Task<InvoiceResponseDto?> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken)
    {
        if (!WorkRecordStatuses.All.Contains(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Status is not supported.");
        }

        var invoice = await context.GetInvoiceForUpdateAsync(id, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        invoice.Status = status;
        invoice.UpdatedAt = timeProvider.GetUtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Map(invoice);
    }

    public async Task<InvoiceOptionsDto> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var clients = await context.GetClientNamesAsync(cancellationToken);
        var projects = await context.GetProjectNamesAsync(cancellationToken);
        return new InvoiceOptionsDto(clients, projects);
    }

    private async Task<Client> GetClientAsync(
        string name,
        CancellationToken cancellationToken)
    {
        return await context.FindClientAsync(NormalizeName(name), cancellationToken)
            ?? throw new ClientNotConfiguredException();
    }

    private async Task<Project> GetOrCreateProjectAsync(
        string name,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        return await context.FindProjectAsync(NormalizeName(name), cancellationToken)
            ?? CreateProject(name, now);
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
            invoice.CreatedAt,
            invoice.GeneratedInvoiceId is not null);
    }
}

public sealed class ClientNotConfiguredException : Exception;
