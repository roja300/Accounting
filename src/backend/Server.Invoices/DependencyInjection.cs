using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Services;
using Server.Invoices.Application.Validators;
using Server.Invoices.Controllers;
using Server.Invoices.Infrastructure;

namespace Server.Invoices;

public static class DependencyInjection
{
    public static IServiceCollection AddInvoicesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddControllers()
            .AddApplicationPart(typeof(InvoiceController).Assembly);
        services.AddDbContext<InvoiceContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(InvoiceContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
            });
        });
        services.AddScoped<IInvoiceContext>(provider =>
            provider.GetRequiredService<InvoiceContext>());
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<InvoiceValidator>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }

    public static async Task ApplyInvoicesMigrationsAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL")))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<InvoiceContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
