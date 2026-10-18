using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Shared.Modules;

public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default);
}
