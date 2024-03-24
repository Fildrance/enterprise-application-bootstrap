using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Builder for entity extractor. Ensures that at least one discover method was added.
/// </summary>
/// <typeparam name="TSelector">Type of selector for entity search in extractr.</typeparam>
/// <typeparam name="TEntity">Type of entity to be found by extractor.</typeparam>
public class EntityExtractorBuilder<TSelector, TEntity> where TEntity : class
{
    #region [Fields]
    private EntityExtractorDiscoverRuleApplyBehavior _behavior = EntityExtractorDiscoverRuleApplyBehavior.OnlyFirstAcceptedRegistration;
    private readonly List<DiscoverRule<TSelector, TEntity>> _discoverRules = new List<DiscoverRule<TSelector, TEntity>>();
    #endregion

    #region [Public]
    #region [Public methods]
    /// <summary>
    /// Adds discover rules.
    /// </summary>
    /// <param name="discoverRules">Array of discover rules to be added for extractor.</param>
    /// <returns>Passed builder with added configuration.</returns>
    public EntityExtractorBuilder<TSelector, TEntity> AddDiscoverRules(params DiscoverRule<TSelector, TEntity>[] discoverRules)
    {
        _discoverRules.AddRange(discoverRules);

        return this;
    }
    /// <summary>
    /// Adds discover rule.
    /// </summary>
    /// <param name="canUse">Func that determines, can this rule be used for passed selector object.</param>
    /// <param name="tryLoad">Func that extracts entity using selector information.</param>
    /// <returns>Passed builder with added configuration.</returns>
    public EntityExtractorBuilder<TSelector, TEntity> AddDiscoverRule(Func<TSelector, bool> canUse, Func<TSelector, DbSet<TEntity>, CancellationToken, Task<TEntity>> tryLoad)
    {
        _discoverRules.Add(new DiscoverRule<TSelector, TEntity>(canUse, tryLoad));

        return this;
    }
    /// <summary>
    /// Creates Entity extractor instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if no discover rule were added.</exception>
    /// <returns>New entity object.</returns>
    public IEntityExtractor<TSelector, TEntity> Build()
    {
        if (_discoverRules.Count == 0)
        {
            throw new InvalidOperationException("Cannot create Entity Extractor without any discover method - such extractor will be invalid.");
        }

        return new SimpleEntityExtractor<TSelector, TEntity>(new List<DiscoverRule<TSelector, TEntity>>(_discoverRules), _behavior);
    }
    /// <summary>
    /// Sets behaviour for entity extractor.
    /// </summary>
    /// <param name="behavior">Default behvaiour for entity discovering.</param>
    /// <returns>Passed builder with added configuration.</returns>
    public EntityExtractorBuilder<TSelector, TEntity> SetBehaviour(EntityExtractorDiscoverRuleApplyBehavior behavior)
    {
        _behavior = behavior;

        return this;
    }
    #endregion
    #endregion
}