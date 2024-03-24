using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Base type for configurations of application endpoints, composed using minimal api.
/// </summary>
public abstract class EndpointRouteConfigureAwareModuleBase : IModule
{
    /// <inheritdoc />
    public abstract string ModuleIdentity { get; }

    /// <summary>
    /// Provider for endpoint configurations. Must return all of endpoints for application. 
    /// </summary>
    [NotNull, ItemNotNull]
    public abstract IEnumerable<IEndpointRouteBuilderAggregator> GetEndpointRouteBuilders(IEndpointRouteBuilder endpointRouteBuilder);

    /// <summary>
    /// Factory method for <see cref="EndpointRouteBuilderAggregator{TService}"/>.
    /// </summary>
    /// <typeparam name="TService">
    /// Service type for which this aggregator should be used. Name of service will be used for all nested
    /// configurations made, for example for 'tags' for swagger.json generation.
    /// </typeparam>
    [NotNull]
    protected static EndpointRouteBuilderAggregator<TService> CreateAggregator<TService>(
        [CanBeNull] Func<IExtendedEndpointRouteBuilder<TService>, IEnumerable<RouteHandlerBuilder>> builder
    ) => new(builder);

    /// <summary>
    /// Aggregator for endpoint builders, enables organized creation for groups of
    /// api with respective additional configurations which can be not duplicated on each route handler builder, but called using <see cref="Override"/> instead.
    /// </summary>
    /// <typeparam name="TService">Service type to which endpoints have to be bound.</typeparam>
    /// <remarks>
    /// Aggregator for endpoint builders, enables organized creation for groups of
    /// api with respective additional configurations which can be not duplicated on each route handler builder, but called using <see cref="Override"/> instead.
    /// </remarks>
    /// <param name="endpoints">
    /// Method for creation of configured route handler builders. It Is recommended to use protected stat
    /// methods of <see cref="IExtendedEndpointRouteBuilder{TService}"/> to configure endpoints - such as <para/>
    /// * <see cref="IExtendedEndpointRouteBuilder{TService}.MapGet{T}"/><para/>
    /// * <see cref="IExtendedEndpointRouteBuilder{TService}.MapPost{T}"/><para/>
    /// * <see cref="IExtendedEndpointRouteBuilder{TService}.MapPut{T}"/><para/>
    /// * <see cref="IExtendedEndpointRouteBuilder{TService}.MapDelete{T}"/>
    /// </param>
    protected class EndpointRouteBuilderAggregator<TService>([CanBeNull] Func<IExtendedEndpointRouteBuilder<TService>, IEnumerable<RouteHandlerBuilder>> endpoints)
        : IEndpointRouteBuilderAggregator
    {
        private readonly List<Action<RouteHandlerBuilder, ServiceRouteMetadata>> _overrideActions = new();

        private readonly List<Action<RouteHandlerBuilder, ServiceRouteMetadata>> _predefineActions = new();

        /// <inheritdoc />
        public void Build(IEndpointRouteBuilder endpointRouteBuilder)
        {
            if (endpoints == null)
            {
                return;
            }

            var extendedEndpointRouteBuilder = new ExtendedEndpointRouteBuilder<TService>(endpointRouteBuilder, _predefineActions);
            var result = endpoints(extendedEndpointRouteBuilder).ToArray();
            foreach (var routeHandlerBuilder in result)
            {
                foreach (var overrideAction in _overrideActions)
                {
                    overrideAction(routeHandlerBuilder, extendedEndpointRouteBuilder.GetMeta(routeHandlerBuilder));
                }
            }
        }

        /// <inheritdoc />
        public IEndpointRouteBuilderAggregator Predefine(Action<RouteHandlerBuilder, ServiceRouteMetadata> predefineAction)
        {
            _predefineActions.Add(predefineAction);
            return this;
        }

        /// <inheritdoc />
        public IEndpointRouteBuilderAggregator Override(Action<RouteHandlerBuilder, ServiceRouteMetadata> overrideAction)
        {
            _overrideActions.Add(overrideAction);
            return this;
        }
    }
}