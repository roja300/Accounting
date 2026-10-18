using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Application.Services;

public sealed class InvoiceService(
    IInvoiceContext context,
    TimeProvider timeProvider) : IInvoiceService
{
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
