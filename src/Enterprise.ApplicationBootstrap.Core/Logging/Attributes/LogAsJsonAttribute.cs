using System;
using MediatR;

namespace Enterprise.ApplicationBootstrap.Core.Logging.Attributes;

/// <summary>
/// Marker for contract types (<see cref="IRequest{TResponse}"/> and <see cref="IRequest"/> -implementing types, Response types) and handler-implementing types
/// (<see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IRequestHandler{TRequest}"/>) for <see cref="LoggingBehaviour{TRequest,TResponse}"/> to
/// know that request / response (or both in case of handler type) should be logged as serialized json using STJ serializer.
/// </summary>
/// <remarks>
/// If request/response type is marked with one attribute, and handler is marked with other - attributes on class to be logged will take priority.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LogAsJsonAttribute : Attribute
{
}