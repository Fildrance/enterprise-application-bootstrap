using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api;
using MediatR;
using Microsoft.Extensions.ObjectPool;

namespace Enterprise.ApplicationBootstrap.Core;

/// <summary>
/// Behaviour that introduces async-local session-store to execution using <see cref="Session"/>.
/// </summary>
public class SessionInitializingBehaviour<TRequest, TResponse>(ObjectPool<Dictionary<string, object>> dictionaryPool) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return Session.ExecuteInSession(() => next(), dictionaryPool);
    }
}