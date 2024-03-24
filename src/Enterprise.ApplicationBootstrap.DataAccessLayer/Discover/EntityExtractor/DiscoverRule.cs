using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Rule for entity discover. 
/// </summary>
/// <typeparam name="TSelector">Type of selector for discover.</typeparam>
/// <typeparam name="TDiscoverResult">Type of discover result (most likely entity).</typeparam>
public class DiscoverRule<TSelector, TDiscoverResult> where TDiscoverResult : class
{
    #region [c-tor]

    /// <summary>
    /// Creates instance of discover rule.
    /// </summary>
    /// <param name="canLoad">
    /// Defines, if selector in passed state can be used for stored discover method, or not.
    /// Method is not supposed to throw any exceptions in casual scenario.
    /// </param>
    /// <param name="tryLoad">
    /// Defines, a way to load entity by selector.
    /// Method is not supposed to throw any exceptions in casual scenario (for example if entity was not found).
    /// </param>
    public DiscoverRule([NotNull] Func<TSelector, bool> canLoad, Func<TSelector, DbSet<TDiscoverResult>, CancellationToken, Task<TDiscoverResult>> tryLoad)
    {
        CanLoad = canLoad;
        TryLoad = tryLoad;
    }

    #endregion

    #region [Public]

    #region [Public properties]

    /// <summary>
    /// Defines, if selector in passed state can be used for stored discover method, or not.
    /// </summary>
    [NotNull]
    public Func<TSelector, bool> CanLoad { get; }

    /// <summary>
    /// Defines, a way to load entity by selector.
    /// </summary>
    [NotNull]
    public Func<TSelector, DbSet<TDiscoverResult>, CancellationToken, Task<TDiscoverResult>> TryLoad { get; }

    #endregion

    #endregion
}
