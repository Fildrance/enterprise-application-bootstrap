using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules;

/// <summary>
/// Module for introducing logic for application request handle pipeline before UseRouting.
/// </summary>
[PublicAPI]
public interface IBeforeRoutingAwareModule : IModule
{
    /// <summary>
    /// Configures application request handle pipeline before UseRouting.
    /// </summary>
    void Configure([NotNull] IApplicationBuilder app);
}