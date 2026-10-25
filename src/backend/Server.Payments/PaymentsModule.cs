using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Payments.Application.Abstractions;
using Server.Payments.Application.Importing;
using Server.Payments.Application.Services;
using Server.Payments.Application.Validation;
using Server.Payments.Controllers;
using Server.Payments.Infrastructure;
using Server.Payments.Infrastructure.Abstractions;
using Server.Shared.Modules;

namespace Server.Payments;

public sealed class PaymentsModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddControllers().AddApplicationPart(typeof(PaymentController).Assembly);
        services.AddDbContext<PaymentContext>(options => options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(PaymentContext).Assembly.FullName);
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
        }));
        services.AddScoped<IPaymentContext>(provider => provider.GetRequiredService<PaymentContext>());
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<CsvPaymentParser>();
        services.AddScoped<PaymentQueryValidator>();
    }

    public async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("PostgreSQL"))) return;
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
