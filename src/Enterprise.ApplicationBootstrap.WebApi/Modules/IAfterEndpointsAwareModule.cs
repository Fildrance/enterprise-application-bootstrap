using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules;

/// <summary>
/// Module that adds option to add logic to application request handle, after UseEndpoints call.
/// </summary>
[PublicAPI]
public interface IAfterEndpointsAwareModule : IModule
{
    /// <summary>
    /// Configures application request handling pipeline after UseEndpoints being called.
    /// </summary>
    void Configure([NotNull] IApplicationBuilder app);
}