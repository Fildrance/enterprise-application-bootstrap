using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Exceptions;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Attributes;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using MediatR;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer;

/// <summary>
/// Mediator behaviour that will control db context and connections/transactions used in handlers.
/// It creates ensures that session have connection/transaction/dbContext created if handler is marked with <see cref="RequireDbContextAttribute"/>.
/// </summary>
public class EntityFrameworkManagingBehaviour<TRequest, TResponse>(IHandlerMetadataStore metadataStore, IDbContextManager dbContextManager)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private const string NoSessionExceptionMessage =
        $"""
         {nameof(EntityFrameworkManagingBehaviour<TRequest, TResponse>)} behaviour is expected to be used after Session is initialized,
         but no session were found in local store (AsyncLocal Session.Current). This may happen if There is no {nameof(SessionInitializingBehaviour<TRequest, TResponse>)}
         were registered or it was registered after {nameof(EntityFrameworkManagingBehaviour<TRequest, TResponse>)}.
         """;


    /// <inheritdoc />
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var currentSession = Session.Current ?? throw new NoSessionFoundException(NoSessionExceptionMessage);

        var metadata = metadataStore.Get<TRequest, TResponse>();
        var requireDbContext = metadata.TryGetAttribute<RequireDbContextAttribute>(out var attribute);
        return requireDbContext
            ? HandleWithAttribute(next, dbContextManager, currentSession, attribute, cancellationToken)
            : next();
    }

    private static async Task<TResponse> HandleWithAttribute(
        RequestHandlerDelegate<TResponse> next,
        IDbContextManager dbContextManager,
        Session currentSession,
        RequireDbContextAttribute attribute,
        CancellationToken cancellationToken
    )
    {
        await dbContextManager.EnsureDbContextExist(currentSession, attribute.WithTransactionIsolationLevel, cancellationToken);
        var result = await next();

        if (!attribute.WithTransactionIsolationLevel.HasValue)
        {
            return result;
        }

        try
        {
            await dbContextManager.Commit(currentSession, cancellationToken);
        }
        catch
        {
            await dbContextManager.Rollback(currentSession, cancellationToken);
            throw;
        }

        return result;
    }
}