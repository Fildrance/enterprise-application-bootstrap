using System;
using MediatR;

namespace Enterprise.ApplicationBootstrap.Core.Logging.Attributes;

/// <summary>
/// Marker for contract types (<see cref="IRequest{TResponse}"/> and <see cref="IRequest"/> -implementing types, Response types) and handler-implementing types
/// (<see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IRequestHandler{TRequest}"/>) for <see cref="LoggingBehaviour{TRequest,TResponse}"/> to
/// know that request / response (or both in case of handler type) should be logged using custom to-string converter. Convert should be registered as <see cref="TConverter"/>
/// </summary>
/// <remarks>
/// If request/response type is marked with one attribute, and handler is marked with other - attributes on class to be logged will take priority.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UseConverterToLogByTypeAttribute<TConverter> : UseConverterToLogByTypeAttribute where TConverter : class, IContractToLogConverter
{
    /// <inheritdoc />
    public UseConverterToLogByTypeAttribute() : base(typeof(TConverter))
    {
    }
}

/// <summary>
/// Marker for contract types (<see cref="IRequest{TResponse}"/> and <see cref="IRequest"/> -implementing types, Response types) and handler-implementing types
/// (<see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IRequestHandler{TRequest}"/>) for <see cref="LoggingBehaviour{TRequest,TResponse}"/> to
/// know that request / response (or both in case of handler type) should be logged using custom to-string converter. Convert should be registered as <see cref="ConverterType"/>
/// </summary>
/// <remarks>
/// If request/response type is marked with one attribute, and handler is marked with other - attributes on class to be logged will take priority.
/// </remarks>
public abstract class UseConverterToLogByTypeAttribute : Attribute
{
    /// <inheritdoc />
    protected internal UseConverterToLogByTypeAttribute(Type converterType)
    {
        ConverterType = converterType;
    }

    /// <summary> Type of converter that should be used for logging contracts. </summary>
    public Type ConverterType { get; }
}