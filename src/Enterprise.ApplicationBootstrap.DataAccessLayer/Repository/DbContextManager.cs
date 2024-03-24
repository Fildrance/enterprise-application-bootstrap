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
/// Default implementation of <see cref="IDbContextManager"/>.
/// </summary>
public class DbContextManager : IDbContextManager
{
    /// <summary> Default key for <see cref="DbContext"/> stored in <see cref="Session"/>. </summary>
    public const string DbContextSessionFieldName = "DbContext";

    /// <summary> Default key for <see cref="DbConnection"/> inside <see cref="Session"/>. </summary>
    public const string EntityFrameworkDefaultConnectionSessionName = "EntityFrameworkDefaultConnection";

    /// <summary> Default key for <see cref="DbTransaction"/> inside <see cref="Session"/>. </summary>
    public const string EntityFrameworkDefaultTransactionSessionName = "EntityFrameworkDefaultTransaction";

    private readonly AsyncManager _asyncManager;
    private readonly SyncManager _syncManager;


    /// <summary> c-tor. </summary>
    public DbContextManager(
        [NotNull] IConnectionFactory connectionFactory,
        [NotNull] IDbContextFactory<CustomDbContext> dbContextFactory,
        [NotNull] IDbContextHelper dbContextHelper
    )
    {
        _asyncManager = new AsyncManager(connectionFactory, dbContextFactory, dbContextHelper);
        _syncManager = new SyncManager(connectionFactory, dbContextFactory, dbContextHelper);
    }

    #region implementation

    /// <inheritdoc />
    public DbContext GetContext(Session session, bool useTransaction = false)
        => _syncManager.GetContext(session, useTransaction);

    /// <inheritdoc />
    public Task<DbContext> GetContextAsync(Session session, CancellationToken ct)
        => _asyncManager.GetContext(session, ct);

    /// <inheritdoc />
    public DbContext GetContextIfExists(Session session)
        => session?.Get<DbContext>(DbContextSessionFieldName);

    /// <inheritdoc />
    public Task Commit(Session session, CancellationToken ct)
    {
        if (session == null)
        {
            throw new NoSessionFoundException();
        }

        var transaction = session.Get<DbTransaction>(EntityFrameworkDefaultTransactionSessionName);
        return transaction?.CommitAsync(ct)
               ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Rollback(Session session, CancellationToken ct)
    {
        if (session == null)
        {
            throw new NoSessionFoundException();
        }

        var transaction = session.Get<DbTransaction>(EntityFrameworkDefaultTransactionSessionName);
        return transaction?.RollbackAsync(ct)
               ?? Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<DbContext> EnsureDbContextExist(Session session, IsolationLevel? isolationLevel, CancellationToken ct)
        => _asyncManager.EnsureDbContext(session, isolationLevel, ct);

    #endregion

    private class AsyncManager(
        IConnectionFactory connectionFactory,
        IDbContextFactory<CustomDbContext> dbContextFactory,
        IDbContextHelper dbContextHelper
    )
    {
        public Task<DbContext> GetContext(Session session, CancellationToken ct = default)
        {
            if (session == null)
            {
                throw new NoSessionFoundException();
            }

            return EnsureDbContext(session, null, ct);
        }

        private async Task<DbTransaction> EnsureHaveTransaction(DbConnection dbConnection, Session session, IsolationLevel isolationLevel, CancellationToken ct)
        {
            var dbTransaction = session.Get<DbTransaction>(EntityFrameworkDefaultTransactionSessionName);
            if (dbTransaction != null)
            {
                return dbTransaction;
            }

            dbTransaction = await CreateNewTransaction(dbConnection, isolationLevel, ct);
            session.Set(EntityFrameworkDefaultTransactionSessionName, dbTransaction);
            return dbTransaction;
        }

        public Task<DbContext> EnsureDbContext(Session session, IsolationLevel? isolationLevel, CancellationToken ct)
        {
            var dbContext = session.Get<DbContext>(DbContextSessionFieldName);
            if (dbContext == null)
            {
                return CreateDbContext(session, isolationLevel, ct);
            }

            return Task.FromResult(dbContext);
        }

        private async Task<DbConnection> EnsureHaveConnection(Session session, CancellationToken ct)
        {
            var dbConnection = session.Get<DbConnection>(EntityFrameworkDefaultConnectionSessionName);
            if (dbConnection != null)
            {
                return dbConnection;
            }

            dbConnection = await connectionFactory.CreateAndOpenConnectionAsync(ct);
            (session.IsRoot ? session : session.Root)
                .Set(EntityFrameworkDefaultConnectionSessionName, dbConnection);
            return dbConnection;
        }

        private async Task<DbContext> CreateDbContext(Session session, IsolationLevel? isolationLevel, CancellationToken ct)
        {
            var connection = await EnsureHaveConnection(session, ct);
            DbContext dbContext = await dbContextFactory.CreateDbContextAsync(ct);
            dbContextHelper.SetConnection(dbContext, connection);
            if (isolationLevel.HasValue)
            {
                var transaction = await EnsureHaveTransaction(connection, session, isolationLevel.Value, ct);
                await dbContextHelper.SetTransactionAsync(dbContext, transaction, ct);
            }

            session.Set(DbContextSessionFieldName, dbContext);
            return dbContext;
        }

        private ValueTask<DbTransaction> CreateNewTransaction(DbConnection dbConnection, IsolationLevel isolationLevel, CancellationToken ct)
        {
            return dbConnection.BeginTransactionAsync(isolationLevel, ct); // add child transaction support?
        }
    }

    private class SyncManager(
        IConnectionFactory connectionFactory,
        IDbContextFactory<CustomDbContext> dbContextFactory,
        IDbContextHelper dbContextHelper
    )
    {
        public DbContext GetContext(Session session, bool useTransaction)
        {
            if (session == null) throw new NoSessionFoundException();

            var dbContext = EnsureDbContext(session, useTransaction);

            return dbContext;
        }

        private DbContext EnsureDbContext(Session session, bool useTransaction)
        {
            var dbContext = session.Get<DbContext>(DbContextSessionFieldName);
            if (dbContext == null)
            {
                return CreateDbContext(session, useTransaction);
            }

            return dbContext;
        }

        private DbConnection EnsureHaveConnection(Session session)
        {
            var dbConnection = session.Get<DbConnection>(EntityFrameworkDefaultConnectionSessionName);
            if (dbConnection != null) return dbConnection;

            dbConnection = connectionFactory.CreateAndOpenConnection();
            (session.IsRoot ? session : session.Root)
                .Set(EntityFrameworkDefaultConnectionSessionName, dbConnection);
            return dbConnection;
        }

        private DbContext CreateDbContext(Session session, bool useTransaction)
        {
            var connection = EnsureHaveConnection(session);

            DbContext dbContext = dbContextFactory.CreateDbContext();
            dbContextHelper.SetConnection(dbContext, connection);
            if (useTransaction)
            {
                var transaction = EnsureHaveTransaction(connection, session);
                dbContextHelper.SetTransaction(dbContext, transaction);
            }

            session.Set(DbContextSessionFieldName, dbContext);
            return dbContext;
        }


        private DbTransaction EnsureHaveTransaction(DbConnection dbConnection, Session session)
        {
            var dbTransaction = session.Get<DbTransaction>(EntityFrameworkDefaultTransactionSessionName);
            if (dbTransaction != null)
            {
                return dbTransaction;
            }

            dbTransaction = CreateNewTransaction(dbConnection);
            session.Set(EntityFrameworkDefaultTransactionSessionName, dbTransaction);
            return dbTransaction;
        }

        private DbTransaction CreateNewTransaction(DbConnection dbConnection)
        {
            return dbConnection.BeginTransaction(); // add child transaction support?
        }
    }
}