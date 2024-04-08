using System;
using Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules;

/// <summary>
/// Aggregator for route handler builders. Can contain multiple configured <see cref="RouteHandlerBuilder"/> and execute preparation and override logic on each of them.
/// </summary>
public interface IEndpointRouteBuilderAggregator
{
    /// <summary> Configures endpoint route handlers. </summary>
    public void Build([NotNull] IEndpointRouteBuilder endpointRouteBuilder);

    /// <summary>
    /// Override for each configuration.
    /// </summary>
    IEndpointRouteBuilderAggregator Override([CanBeNull] Action<RouteHandlerBuilder, ServiceRouteMetadata> overrideAction);

    /// <summary>
    /// Preparation call for each route-handler builder.
    /// </summary>
    IEndpointRouteBuilderAggregator Predefine([CanBeNull] Action<RouteHandlerBuilder, ServiceRouteMetadata> predefineAction);
}