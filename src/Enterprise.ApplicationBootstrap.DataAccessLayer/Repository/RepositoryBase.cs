using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Exceptions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> Base class for repository of certain entities. </summary>
/// <typeparam name="TEntity">Type of entity.</typeparam>
/// <typeparam name="TId">Type of id for entity.</typeparam>
[PublicAPI]
public abstract class RepositoryBase<TEntity, TId>(IDbContextManager dbContextManager) : IRepository<TEntity, TId>
    where TEntity : EntityBase<TId>
{
    private const string NoSessionErrorMessage = "Failed to resolve DbContext from current session - no session found in async local Session.Current.";

    private readonly IDbContextManager _dbContextManager = dbContextManager ?? throw new ArgumentNullException(nameof(dbContextManager));

    /// <summary>
    /// Extracts dbContext from current Session.
    /// </summary>
    protected DbContext GetContext()
        => _dbContextManager.GetContext(Session.Current ?? throw new NoSessionFoundException(NoSessionErrorMessage));

    /// <summary>
    /// Extracts dbContext from current Session.
    /// </summary>
    protected Task<DbContext> GetContextAsync(CancellationToken ct)
        => _dbContextManager.GetContextAsync(Session.Current ?? throw new NoSessionFoundException(NoSessionErrorMessage), ct);

    /// <summary>
    /// Get DbSet of entities of <see cref="TEntity"/> from <see cref="GetContext"/>.
    /// </summary>
    protected DbSet<TEntity> GetSet() => GetContext().Set<TEntity>();

    /// <inheritdoc />
    public async Task Add(TEntity entity, CancellationToken ct)
    {
        var context = await GetContextAsync(ct);
        await context.Set<TEntity>()
                     .AddAsync(entity, ct);
        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task Remove(TEntity entity, CancellationToken ct)
    {
        var context = await GetContextAsync(ct);
        context.Set<TEntity>()
               .Remove(entity);
        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task Update(TEntity entity, CancellationToken ct)
    {
        var context = await GetContextAsync(ct);
        context.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<TEntity> GetById(TId id, CancellationToken ct)
    {
        var dbContext = await GetContextAsync(ct);
        return await dbContext.Set<TEntity>()
                              .FirstAsync(x => x.Id.Equals(id), cancellationToken: ct);
    }
}