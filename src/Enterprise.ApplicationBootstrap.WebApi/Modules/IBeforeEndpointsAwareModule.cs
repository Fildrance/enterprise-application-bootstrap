using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules;

/// <summary>
/// Module for introducing logic for application request handle pipeline before UseEndpoints was called and after UseRouting was called.
/// </summary>
[PublicAPI]
public interface IBeforeEndpointsAwareModule : IModule
{
    /// <summary>
    /// Configures application request handle pipeline before UseEndpoints was called and after UseRouting was called.
    /// </summary>
    void Configure([NotNull]IApplicationBuilder app);
}