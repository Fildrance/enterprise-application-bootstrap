using MediatR;

namespace Enterprise.ApplicationBootstrap.Core.HandlerMetadata;

/// <summary>
/// Store for <see cref="IRequestHandler{TRequest,TResponse}"/> and <see cref="IRequestHandler{TRequest}"/> implementations metadata (attributes).
/// </summary>
public interface IHandlerMetadataStore
{
    /// <summary>
    /// Gets metadata container for request handler that suites passed request and response types.
    /// </summary>
    HandlerMetadataInfo Get<TRequest, TResponse>() where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Gets metadata container for request handler that suites passed request type.
    /// </summary>
    HandlerMetadataInfo Get<TRequest>() where TRequest : IRequest;
}