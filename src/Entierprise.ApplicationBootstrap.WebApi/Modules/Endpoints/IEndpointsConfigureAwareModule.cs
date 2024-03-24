using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Introduces a way to customize endpoints configuration of application request handling pipeline.
/// Is more 'raw' alternative to <see cref="EndpointRouteConfigureAwareModuleBase"/>, less preferred.
/// Configurations made here won't have <see cref="OpenAPi.DefaultApiVersionModule"/> etc.
/// </summary>
[PublicAPI]
public interface IEndpointsConfigureAwareModule : IModule
{
    /// <summary>
    /// Executes <see cref="IEndpointRouteBuilder"/> dependent logic.
    /// </summary>
    void Configure([NotNull] IEndpointRouteBuilder endpointRouteBuilder);
}