using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Abstractions;

public interface IExportedInvoiceRepository
{
    Task<BusinessProfile?> GetBusinessProfileAsync(CancellationToken cancellationToken);

    Task<InvoiceConfiguration?> GetInvoiceConfigurationAsync(CancellationToken cancellationToken);

    Task<Client?> GetClientAsync(Guid id, CancellationToken cancellationToken);

    Task<Project?> GetProjectAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Invoice>> GetEligibleWorkRecordsAsync(
        Guid clientId,
        Guid? projectId,
        string currency,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Client>> GetClientsWithEligibleWorkAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetProjectsWithEligibleWorkAsync(CancellationToken cancellationToken);

    Task AddGeneratedInvoiceAsync(GeneratedInvoice invoice, CancellationToken cancellationToken);

    Task<GeneratedInvoice?> GetGeneratedInvoiceAsync(
        Guid id,
        bool forUpdate,
        CancellationToken cancellationToken);

    Task<GeneratedInvoiceQueryResult> QueryGeneratedInvoicesAsync(
        GeneratedInvoiceQuery query,
        CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
