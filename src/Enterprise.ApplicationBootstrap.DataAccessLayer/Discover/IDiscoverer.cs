using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover;

/// <summary>
/// Can discover entities by selector.
/// </summary>
/// <typeparam name="TSelector">Type of selector for searching of entities.</typeparam>
/// <typeparam name="TEntity">Type of entity to be found.</typeparam>
public interface IDiscoverer<in TSelector, TEntity> where TEntity : class
{
    /// <summary>
    /// Searches entity by mentioned selector.
    /// </summary>
    /// <param name="selector">Selector for search.</param>
    /// <exception cref="InvalidOperationException">Is thrown when passed selector does not match any registered discover rule.</exception>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Found entity or null if entity not found.</returns>
    [NotNull, ItemCanBeNull]
    Task<TEntity> DiscoverAsync([NotNull] TSelector selector, CancellationToken ct);

    /// <summary>
    /// Prepares discoverer for work.
    /// </summary>
    void ConfigureExtractor();
}