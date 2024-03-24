using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.HandlerMetadata;

/// <summary>
/// Default <see cref="IHandlerMetadataStore"/> implementation. Can be created by <seealso cref="Builder"/>.
/// </summary>
public sealed class HandlerMetadataStore : IHandlerMetadataStore
{
    private readonly Dictionary<Type, HandlerMetadataInfo> _attributesByHandlerInterfaceType;

    private HandlerMetadataStore(Dictionary<Type, Type> attributesByHandlerInterfaceType)
    {
        _attributesByHandlerInterfaceType = attributesByHandlerInterfaceType.ToDictionary(
            x => x.Key,
            x => new HandlerMetadataInfo(x.Value)
        );
    }

    /// <summary>
    /// Returns metadata info for request handler that was registered in DI container.
    /// </summary>
    /// <typeparam name="TRequest">Request type for handler.</typeparam>
    /// <typeparam name="TResponse">Response type for handler.</typeparam>
    /// <returns>Found metadata or null.</returns>
    public HandlerMetadataInfo Get<TRequest, TResponse>() where TRequest : IRequest<TResponse>
    {
        _attributesByHandlerInterfaceType.TryGetValue(typeof(IRequestHandler<TRequest, TResponse>), out var meta);
        return meta;
    }

    /// <inheritdoc />
    public HandlerMetadataInfo Get<TRequest>() where TRequest : IRequest
    {
        _attributesByHandlerInterfaceType.TryGetValue(typeof(IRequestHandler<TRequest>), out var meta);
        return meta;
    }

    /// <summary> Builder for creation. </summary>
    public sealed class Builder
    {
        private readonly Type[] _registeredHandlerInterfaces;

        /// <summary>
        /// Initializes builder for <see cref="HandlerMetadataStore"/>.
        /// Should be initialized after all registrations on <seealso cref="IServiceCollection"/> were called.
        ///  </summary>
        /// <remarks> This call is not mutating and won't change <see cref="IServiceCollection"/> contents.</remarks>
        public Builder(IServiceCollection services)
        {
            var requestHandlerInterface = typeof(IRequestHandler<>);
            var requestHandlerInterface2 = typeof(IRequestHandler<,>);
            _registeredHandlerInterfaces = services.Where(
                                                       x =>
                                                       {
                                                           if (!x.ServiceType.IsConstructedGenericType)
                                                           {
                                                               return false;
                                                           }

                                                           var gd = x.ServiceType.GetGenericTypeDefinition();
                                                           return gd == requestHandlerInterface || gd == requestHandlerInterface2;
                                                       })
                                                   .Select(x => x.ServiceType)
                                                   .ToArray();
        }

        /// <summary>
        /// Creates <see cref="HandlerMetadataStore"/>. Will create every instance of <see cref="IRequestHandler{TRequest,TResponse}"/>
        /// and <see cref="IRequestHandler{TRequest}"/> that is registered in <see cref="IServiceCollection"/> from <see cref="IServiceProvider"/>
        /// to store implementation <see cref="Type"/>.
        /// </summary>
        public HandlerMetadataStore Build(IServiceProvider serviceProvider)
        {
            Dictionary<Type, Type> handlerImplementationTypesByInterfaceTypes = new();
            var logger = serviceProvider.GetRequiredService<ILogger<Builder>>();
            foreach (var registeredHandlerInterface in _registeredHandlerInterfaces)
            {
                var implementation = serviceProvider.GetService(registeredHandlerInterface);
                if (implementation == null)
                {
                    logger.LogWarning(
                        "Failed to get instance for '{interface}' from service provider on application start. "
                        + "HandlerMetadataStore requires to get instance for each working handler to pre-store their attributes.",
                        registeredHandlerInterface.FullName
                    );
                    continue;
                }

                handlerImplementationTypesByInterfaceTypes.Add(registeredHandlerInterface, implementation.GetType());
            }

            return new HandlerMetadataStore(handlerImplementationTypesByInterfaceTypes);
        }
    }
}