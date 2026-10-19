using Microsoft.EntityFrameworkCore;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure.Abstractions;

namespace Server.Invoices.Infrastructure;

public sealed class InvoiceContext(DbContextOptions<InvoiceContext> options)
    : DbContext(options), IInvoiceContext
{
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public async Task<InvoiceQueryResult> QueryInvoicesAsync(
        InvoiceQuery query,
        CancellationToken cancellationToken)
    {
        var invoices = Invoices.AsNoTracking();

        if (query.CreatedOnOrAfter is not null)
        {
            invoices = invoices.Where(invoice => invoice.CreatedAt >= query.CreatedOnOrAfter);
        }

        if (query.CreatedOnOrBefore is not null)
        {
            invoices = invoices.Where(invoice => invoice.CreatedAt <= query.CreatedOnOrBefore);
        }

        if (query.NormalizedClient is not null)
        {
            invoices = invoices.Where(
                invoice => invoice.Client.NormalizedName == query.NormalizedClient);
        }

        if (query.NormalizedProject is not null)
        {
            invoices = invoices.Where(
                invoice => invoice.Project.NormalizedName == query.NormalizedProject);
        }

        if (query.DescriptionSearch is not null)
        {
            var pattern = $"%{EscapeLikePattern(query.DescriptionSearch)}%";
            invoices = invoices.Where(
                invoice => EF.Functions.ILike(invoice.Description, pattern, "\\"));
        }

        var totalItems = await invoices.CountAsync(cancellationToken);
        if (query.Offset > int.MaxValue)
        {
            return new InvoiceQueryResult([], totalItems);
        }

        var items = await invoices
            .Include(invoice => invoice.Client)
            .Include(invoice => invoice.Project)
            .OrderByDescending(invoice => invoice.CreatedAt)
            .ThenByDescending(invoice => invoice.Id)
            .Skip((int)query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return new InvoiceQueryResult(items, totalItems);
    }

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

    public Task<Invoice?> GetInvoiceForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Invoices
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

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
