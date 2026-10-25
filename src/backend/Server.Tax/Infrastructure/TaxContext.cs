using Microsoft.EntityFrameworkCore;
using Server.Tax.Domain;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Infrastructure;

public sealed class TaxContext(DbContextOptions<TaxContext> options)
    : DbContext(options), ITaxYearRepository
{
    public DbSet<TaxYearConfiguration> TaxYearConfigurations => Set<TaxYearConfiguration>();

    public async Task<IReadOnlyList<TaxYearConfiguration>> GetConfigurationsAsync(
        CancellationToken cancellationToken) =>
        await TaxYearConfigurations.AsNoTracking().OrderByDescending(value => value.StartYear).ToListAsync(cancellationToken);

    public Task<TaxYearConfiguration?> GetConfigurationAsync(
        int startYear,
        CancellationToken cancellationToken) =>
        TaxYearConfigurations.SingleOrDefaultAsync(value => value.StartYear == startYear, cancellationToken);

    public async Task AddConfigurationAsync(
        TaxYearConfiguration configuration,
        CancellationToken cancellationToken) =>
        await TaxYearConfigurations.AddAsync(configuration, cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaxContext).Assembly);
    }
}
