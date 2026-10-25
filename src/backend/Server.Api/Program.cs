using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Server.Api.Health;
using Server.Invoices;
using Server.Payments;
using Server.Shared.Modules;
using Server.Tax;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var httpsRedirectEnabled = builder.Configuration.GetValue("Https:RedirectEnabled", true);
var httpsPort = builder.Configuration.GetValue<int?>("Https:Port");

List<IModule> modules =
[
    new InvoicesModule(),
    new PaymentsModule(),
    new TaxModule(),
];

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

builder.Services.AddOpenApi();
builder.Services.AddHttpsRedirection(options => options.HttpsPort = httpsPort);
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<PostgreSqlHealthCheck>(
        "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var app = builder.Build();

foreach (var module in modules)
{
    await module.InitializeAsync(
        app.Services,
        builder.Configuration,
        app.Lifetime.ApplicationStopping);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (httpsRedirectEnabled)
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});
app.Run();

public partial class Program;
