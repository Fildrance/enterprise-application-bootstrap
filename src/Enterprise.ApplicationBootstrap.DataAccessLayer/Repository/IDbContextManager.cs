using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Exceptions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using IsolationLevel = System.Data.IsolationLevel;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary>
/// Manager for EF Core <see cref="DbContext"/>. Must be used with <seealso cref="Session"/> as storage for <see cref="DbContext"/> and db-connection/transaction.
/// </summary>
public interface IDbContextManager
{
    /// <summary>
    /// Tries to get DbContext from <paramref name="session"/>. If none found - tries to get DbConnection from <paramref name="session"/> and create DbContext using it.
    /// If none found - creates <see cref="DbConnection"/>, sets it into <paramref name="session"/> and then uses it to create DbContext.
    /// </summary>
    /// <param name="session">Session which must be used as store for DbContext.</param>
    /// <param name="useTransaction">Marker that dbContext should use transaction during current session.</param>
    /// <returns>Created (or found existing) DbContext.</returns>
    /// <exception cref="NoSessionFoundException">Throws if session is null.</exception>
    [NotNull]
    DbContext GetContext([NotNull] Session session, bool useTransaction = false);

    /// <summary>
    /// Tries to get DbContext from <paramref name="session"/>. If none found - tries to get DbConnection from <paramref name="session"/> and create DbContext using it.
    /// If none found - creates <see cref="DbConnection"/>, sets it into <paramref name="session"/> and then uses it to create DbContext.
    /// </summary>
    /// <param name="session">Session which must be used as store for DbContext.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Created (or found existing) DbContext.</returns>
    /// <exception cref="NoSessionFoundException">Throws if session is null.</exception>
    [NotNull, ItemNotNull]
    Task<DbContext> GetContextAsync([NotNull] Session session, CancellationToken ct);

    /// <summary>
    /// Tries to get DbContext from <paramref name="session"/>.
    /// </summary>
    /// <param name="session">Session which must be used as store for DbContext.</param>
    /// <returns>Found DbContext or null.</returns>
    [CanBeNull]
    DbContext GetContextIfExists([CanBeNull] Session session);

    /// <summary>
    /// Commits current transaction (that exists inside <paramref name="session"/>). If no transaction found - will do nothing.
    /// </summary>
    /// <param name="session">Session that is used as store for transaction to be committed.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    [NotNull]
    Task Commit([NotNull] Session session, CancellationToken ct);

    /// <summary>
    /// Rolls back transaction (that exists inside <paramref name="session"/>). If no transaction found - will do nothing.
    /// </summary>
    /// <param name="session">Session that is used as store for transaction to be committed.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    [NotNull]
    Task Rollback([NotNull] Session session, CancellationToken ct);

    /// <summary>
    /// Ensures there is DbContext in session (and returns same one).
    /// </summary>
    /// <param name="session">Session that is used as store for transaction to be committed.</param>
    /// <param name="isolationLevel">Isolation level for required transaction. If null - no transaction will be set.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Found DbContext or created one.</returns>
    [NotNull, ItemNotNull]
    Task<DbContext> EnsureDbContextExist([NotNull] Session session, IsolationLevel? isolationLevel, CancellationToken ct);
}