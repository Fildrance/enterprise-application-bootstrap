using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.WebApi.Sample.Modules;

internal class SampleModule : IServiceCollectionAwareModule
{
    /// <inheritdoc />
    public string ModuleIdentity => "SampleModule";

    /// <inheritdoc />
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SampleService>();
    }
}