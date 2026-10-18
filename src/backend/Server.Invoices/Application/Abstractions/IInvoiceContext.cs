using Server.Invoices.Domain;

namespace Server.Invoices.Application.Abstractions;

public interface IInvoiceContext
{
    Task<Client?> FindClientAsync(string normalizedName, CancellationToken cancellationToken);

    Task<Project?> FindProjectAsync(string normalizedName, CancellationToken cancellationToken);

    Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken);

    Task<Invoice?> GetInvoiceAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetClientNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetProjectNamesAsync(CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
