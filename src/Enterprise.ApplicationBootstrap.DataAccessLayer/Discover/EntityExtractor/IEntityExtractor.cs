using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Extractor for entities. Can discover entity object from ORM-repository using
/// selector, that contains some fields, based on which discover rule will be picked and executed in Extractor internals.
/// </summary>
/// <typeparam name="TSelector">Type of selector for entity search.</typeparam>
/// <typeparam name="TEntity">Type of entity to be found.</typeparam>
public interface IEntityExtractor<in TSelector, TEntity> where TEntity : class
{
    /// <summary>
    /// Searches entity by mentioned selector.
    /// </summary>
    /// <param name="selector">Selector for search.</param>
    /// <param name="dbSet">EF DbSet of entity from which discover should occur.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <exception cref="InvalidOperationException">Is thrown when passed selector does not match any registered discover rule.</exception>
    /// <returns>Found entity or null if entity not found.</returns>
    [NotNull]
    Task<TEntity> ExtractAsync([NotNull] TSelector selector, [NotNull] DbSet<TEntity> dbSet, CancellationToken ct);
}