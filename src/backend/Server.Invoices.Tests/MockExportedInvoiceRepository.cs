using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Tests;

internal sealed class MockExportedInvoiceRepository : IExportedInvoiceRepository
{
    public BusinessProfile? BusinessProfile { get; set; }

    public Client? Client { get; set; }

    public Project? Project { get; set; }

    public IReadOnlyList<Invoice> EligibleWorkRecords { get; set; } = [];

    public IReadOnlyList<GeneratedInvoice> GeneratedInvoices { get; set; } = [];

    public GeneratedInvoice? StoredGeneratedInvoice { get; set; }

    public GeneratedInvoiceQuery? CapturedQuery { get; private set; }

    public GeneratedInvoice? AddedGeneratedInvoice { get; private set; }

    public int SaveChangesCalls { get; private set; }

    public Task<BusinessProfile?> GetBusinessProfileAsync(CancellationToken cancellationToken) =>
        Task.FromResult(BusinessProfile);

    public Task<Client?> GetClientAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Client?.Id == id ? Client : null);

    public Task<Project?> GetProjectAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Project?.Id == id ? Project : null);

    public Task<IReadOnlyList<Invoice>> GetEligibleWorkRecordsAsync(
        Guid clientId,
        Guid? projectId,
        string currency,
        CancellationToken cancellationToken) => Task.FromResult(EligibleWorkRecords);

    public Task<IReadOnlyList<Client>> GetClientsWithEligibleWorkAsync(
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Client>>(Client is null ? [] : [Client]);

    public Task<IReadOnlyList<Project>> GetProjectsWithEligibleWorkAsync(
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Project>>(Project is null ? [] : [Project]);

    public Task AddGeneratedInvoiceAsync(
        GeneratedInvoice invoice,
        CancellationToken cancellationToken)
    {
        AddedGeneratedInvoice = invoice;
        return Task.CompletedTask;
    }

    public Task<GeneratedInvoice?> GetGeneratedInvoiceAsync(
        Guid id,
        bool forUpdate,
        CancellationToken cancellationToken) =>
        Task.FromResult(StoredGeneratedInvoice?.Id == id ? StoredGeneratedInvoice : null);

    public Task<GeneratedInvoiceQueryResult> QueryGeneratedInvoicesAsync(
        GeneratedInvoiceQuery query,
        CancellationToken cancellationToken)
    {
        CapturedQuery = query;
        return Task.FromResult(new GeneratedInvoiceQueryResult(
            GeneratedInvoices,
            GeneratedInvoices.Count));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
