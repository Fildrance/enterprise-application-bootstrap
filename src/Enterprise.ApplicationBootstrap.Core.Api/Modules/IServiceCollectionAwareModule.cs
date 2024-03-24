using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.Core.Api.Modules;

/// <summary> Module for registering dependencies. </summary>
[PublicAPI]
public interface IServiceCollectionAwareModule : IModule
{
    /// <summary>
    /// Registers services in DI container.
    /// </summary>
    /// <param name="services">DI Container.</param>
    /// <param name="configuration">Application configuration.</param>
    void Configure([NotNull] IServiceCollection services, [NotNull] IConfiguration configuration);
}