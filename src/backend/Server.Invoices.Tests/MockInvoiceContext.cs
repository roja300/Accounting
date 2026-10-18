using Server.Invoices.Application.Abstractions;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Tests;

internal sealed class MockInvoiceContext : IInvoiceContext
{
    public IReadOnlyList<Invoice> Invoices { get; set; } = [];

    public InvoiceQuery? CapturedQuery { get; private set; }

    public Client? Client { get; set; }

    public Project? Project { get; set; }

    public Invoice? AddedInvoice { get; private set; }

    public int SaveChangesCalls { get; private set; }

    public Task<InvoiceQueryResult> QueryInvoicesAsync(
        InvoiceQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CapturedQuery = query;
        return Task.FromResult(new InvoiceQueryResult(Invoices, Invoices.Count));
    }

    public Task<Client?> FindClientAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            Client?.NormalizedName == normalizedName ? Client : null);
    }

    public Task<Project?> FindProjectAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            Project?.NormalizedName == normalizedName ? Project : null);
    }

    public Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AddedInvoice = invoice;
        return Task.CompletedTask;
    }

    public Task<Invoice?> GetInvoiceAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(AddedInvoice?.Id == id ? AddedInvoice : null);
    }

    public Task<IReadOnlyList<string>> GetClientNamesAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<string> clients = Client is null ? [] : [Client.Name];
        return Task.FromResult(clients);
    }

    public Task<IReadOnlyList<string>> GetProjectNamesAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<string> projects = Project is null ? [] : [Project.Name];
        return Task.FromResult(projects);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
