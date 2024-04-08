using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Module that will call same logic on every <see cref="RouteHandlerBuilder"/> created in modules, derived from <see cref="EndpointRouteConfigureAwareModuleBase"/>.
/// </summary>
public interface IRouteHandlerBuilderAwareModule : IModule
{
    /// <summary>
    /// Configures RouteHandler. Is called for every route-handler created in <see cref="EndpointRouteConfigureAwareModuleBase"/>-derived types.
    /// </summary>
    /// <param name="routeHandlerBuilder">Builder for route handler.</param>
    /// <param name="metadata">
    /// Service call metadata. <see cref="EndpointRouteConfigureAwareModuleBase"/>-derived modules create route handlers
    /// which should call <see cref="IMediator"/>-related handler. Details of this service call is nested in metadata.
    /// </param>
    void ConfigureRoutHandlers([NotNull] RouteHandlerBuilder routeHandlerBuilder, [NotNull] ServiceRouteMetadata metadata);
}