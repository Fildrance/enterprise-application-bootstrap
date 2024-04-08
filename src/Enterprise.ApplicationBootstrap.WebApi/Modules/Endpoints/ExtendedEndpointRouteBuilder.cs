using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Builder for MinimalAPI routes.
/// It can be used to delegate overriding/pre-configuration to modules
/// </summary>
/// <param name="builder">Endpoint builder of application.</param>
/// <param name="predefineActions">Pre-configuration that should be executed before any overrides.</param>
/// <typeparam name="TService">Type of service, that will be used in handling of route.</typeparam>
public class ExtendedEndpointRouteBuilder<TService>(
    [NotNull] IEndpointRouteBuilder builder,
    [CanBeNull] IReadOnlyCollection<Action<RouteHandlerBuilder, ServiceRouteMetadata>> predefineActions
) : IExtendedEndpointRouteBuilder<TService>
{
    private readonly Dictionary<RouteHandlerBuilder, ServiceRouteMetadata> _calls = new();
    private readonly IReadOnlyCollection<Action<RouteHandlerBuilder, ServiceRouteMetadata>> _predefinedActions = predefineActions;

    /// <summary>
    /// Extracts handler delegate metadata from route handle that was configured earlier in same instance of <see cref="ExtendedEndpointRouteBuilder{TService}"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">When <paramref name="builder"/> is null.</exception>
    public ServiceRouteMetadata GetMeta([NotNull] RouteHandlerBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return _calls[builder];
    }

    #region registration helpers

    /// <inheritdoc />
    public IExtendedEndpointRouteBuilder<TService, TRequest> MapGet<TRequest>(string route) where TRequest : IBaseRequest
        => new NestedExtendedEndpointRouteBuilder<TRequest>(this, @delegate => builder.MapGet(route, @delegate), false);

    /// <inheritdoc />
    public IExtendedEndpointRouteBuilder<TService, TRequest> MapPost<TRequest>(string route) where TRequest : IBaseRequest
        => new NestedExtendedEndpointRouteBuilder<TRequest>(this, @delegate => builder.MapPost(route, @delegate), true);

    /// <inheritdoc />
    public IExtendedEndpointRouteBuilder<TService, TRequest> MapDelete<TRequest>(string route) where TRequest : IBaseRequest
        => new NestedExtendedEndpointRouteBuilder<TRequest>(this, @delegate => builder.MapDelete(route, @delegate), false);

    /// <inheritdoc />
    public IExtendedEndpointRouteBuilder<TService, TRequest> MapPut<TRequest>(string route) where TRequest : IBaseRequest
        => new NestedExtendedEndpointRouteBuilder<TRequest>(this, @delegate => builder.MapPut(route, @delegate), true);

    /// <inheritdoc />
    public IExtendedEndpointRouteBuilder<TService, TRequest> MapPatch<TRequest>(string route) where TRequest : IBaseRequest
        => new NestedExtendedEndpointRouteBuilder<TRequest>(this, @delegate => builder.MapPatch(route, @delegate), true);

    #endregion

    /// <summary>
    /// Helper that executes registration of route handler.
    /// Is used to ease type inference (to not write type of <see cref="TRequest"/>, as it will require also putting TResponse directly).
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="original">Original builder.</param>
    /// <param name="func">Delegate that will create route handler.</param>
    /// <param name="fromBody">Marker, is contract passed through request body or query params.</param>
    private class NestedExtendedEndpointRouteBuilder<TRequest>(
        [NotNull] ExtendedEndpointRouteBuilder<TService> original,
        [NotNull] Func<Delegate, RouteHandlerBuilder> func,
        bool fromBody
    ) : IExtendedEndpointRouteBuilder<TService, TRequest>
        where TRequest : IBaseRequest
    {
        /// <inheritdoc />
        public RouteHandlerBuilder To<TResponse>(Expression<Func<TService, TRequest, CancellationToken, TResponse>> handleExpression)
        {
            if (handleExpression == null)
            {
                throw new ArgumentNullException(nameof(handleExpression));
            }

            var handleDelegate = handleExpression.Compile();
            Delegate @delegate;
            if (fromBody)
            {
                @delegate = (TRequest request, CancellationToken ct, [FromServices] TService srv) => handleDelegate(srv, request, ct);
            }
            else
            {
                @delegate = ([AsParameters] TRequest request, CancellationToken ct, [FromServices] TService srv) => handleDelegate(srv, request, ct);
            }

            var routeHandlerBuilder = func(@delegate);

            var (calledMethodName, serviceName) = GetMeta(handleExpression);
            var serviceRouteMetadata = new ServiceRouteMetadata(calledMethodName);
            original._calls.Add(routeHandlerBuilder, serviceRouteMetadata);
            foreach (var originalPredefinedAction in original._predefinedActions)
            {
                originalPredefinedAction(routeHandlerBuilder, serviceRouteMetadata);
            }
            return routeHandlerBuilder;
        }

        private static (string MethodName, string ServiceName) GetMeta<TResponse>(Expression<Func<TService, TRequest, CancellationToken, TResponse>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;
            return (MethodName: methodCallExpression.Method.Name, typeof(TService).FullName);
        }
    }

    /// <inheritdoc />
    public IApplicationBuilder CreateApplicationBuilder()
    {
        return builder.CreateApplicationBuilder();
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider => builder.ServiceProvider;

    /// <inheritdoc />
    public ICollection<EndpointDataSource> DataSources => builder.DataSources;
}