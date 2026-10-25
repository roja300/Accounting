using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Shared.Modules;
using Server.Tax.Application.Abstractions;
using Server.Tax.Application.Services;
using Server.Tax.Controllers;
using Server.Tax.Infrastructure;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax;

public sealed class TaxModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddControllers().AddApplicationPart(typeof(TaxYearController).Assembly);
        services.AddDbContext<TaxContext>(options => options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(TaxContext).Assembly.FullName);
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
        }));
        services.AddScoped<ITaxYearRepository>(provider => provider.GetRequiredService<TaxContext>());
        services.AddScoped<ITaxIncomeSource, InvoiceTaxIncomeSource>();
        services.AddScoped<ITaxYearService, TaxYearService>();
    }

    public async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL"))) return;
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TaxContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
