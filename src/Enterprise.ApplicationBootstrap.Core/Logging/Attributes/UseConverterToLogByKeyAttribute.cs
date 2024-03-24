using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.Core.Logging.Attributes;

/// <summary>
/// Marker for contract types (<see cref="IRequest{TResponse}"/> and <see cref="IRequest"/> -implementing types, Response types) and handler-implementing types
/// (<see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IRequestHandler{TRequest}"/>) for <see cref="LoggingBehaviour{TRequest,TResponse}"/> to
/// know that request / response (or both in case of handler type) should be logged using custom to-string converter. Convert should be registered
/// as <see cref="IContractToLogConverterProvider"/> using <see cref="ServiceCollectionServiceExtensions.AddKeyedScoped{TService,TImplementation}(IServiceCollection,object?)"/>.
/// </summary>
/// <remarks>
/// If request/response type is marked with one attribute, and handler is marked with other - attributes on class to be logged will take priority.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UseConverterToLogByKeyAttribute : Attribute
{
    /// <inheritdoc />
    public UseConverterToLogByKeyAttribute(object converterKey)
    {
        ConverterKey = converterKey;
    }

    /// <summary>
    /// Key for resolving converter from Container.
    /// </summary>
    public object ConverterKey { get; }
}