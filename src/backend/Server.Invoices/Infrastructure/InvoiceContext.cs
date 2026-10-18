using Microsoft.EntityFrameworkCore;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure;

public sealed class InvoiceContext(DbContextOptions<InvoiceContext> options)
    : DbContext(options), IInvoiceContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public Task<Client?> FindClientAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return Clients.SingleOrDefaultAsync(
            client => client.NormalizedName == normalizedName,
            cancellationToken);
    }

    public Task<Project?> FindProjectAsync(
        string normalizedName,
        CancellationToken cancellationToken)
    {
        return Projects.SingleOrDefaultAsync(
            project => project.NormalizedName == normalizedName,
            cancellationToken);
    }

    public async Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        await Invoices.AddAsync(invoice, cancellationToken);
    }

    public Task<Invoice?> GetInvoiceAsync(Guid id, CancellationToken cancellationToken)
    {
        return Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Client)
            .Include(invoice => invoice.Project)
            .SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetClientNamesAsync(
        CancellationToken cancellationToken)
    {
        return await Clients
            .AsNoTracking()
            .OrderBy(client => client.Name)
            .Select(client => client.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetProjectNamesAsync(
        CancellationToken cancellationToken)
    {
        return await Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .Select(project => project.Name)
            .ToListAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceContext).Assembly);
    }
}
