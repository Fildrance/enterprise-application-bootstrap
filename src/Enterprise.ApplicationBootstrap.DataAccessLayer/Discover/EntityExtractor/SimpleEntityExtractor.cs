using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor.Exceptions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Extractor for entities.
/// </summary>
/// <typeparam name="TSelector">Type of selector for entity search.</typeparam>
/// <typeparam name="TEntity">Type of entity to be searched.</typeparam>
public class SimpleEntityExtractor<TSelector, TEntity> : IEntityExtractor<TSelector, TEntity> where TEntity : class
{
    #region [Fields]

    private readonly EntityExtractorDiscoverRuleApplyBehavior _behavior;
    private readonly IReadOnlyCollection<DiscoverRule<TSelector, TEntity>> _registrations;

    #endregion

    #region [c-tor]

    /// <summary>
    /// Creates instance of entity extractor.
    /// </summary>
    protected internal SimpleEntityExtractor(
        [NotNull] IReadOnlyCollection<DiscoverRule<TSelector, TEntity>> registrations,
        EntityExtractorDiscoverRuleApplyBehavior behavior = EntityExtractorDiscoverRuleApplyBehavior.OnlyFirstAcceptedRegistration
    )
    {
        _registrations = registrations;
        _behavior = behavior;
    }

    #endregion

    #region IEntityExtractor<TSelector,TEntity> implementation

    /// <inheritdoc />
    public async Task<TEntity> ExtractAsync(TSelector selector, DbSet<TEntity> dbSet, CancellationToken ct)
    {
        var rules = EnumerateAcceptableDiscoverRules(selector);

        bool attemptedResolve = false;
        foreach (var registration in rules)
        {
            attemptedResolve = true;
            var tryLoadResult = registration.TryLoad(selector, dbSet, ct);
            if (tryLoadResult != null)
            {
                return await tryLoadResult;
            }
        }

        if (!attemptedResolve)
        {
            throw new CannotFindEntityDiscoverRuleException(selector);
        }

        return null;
    }

    #endregion

    #region [Private]

    #region [Private methods]

    private IEnumerable<DiscoverRule<TSelector, TEntity>> EnumerateAcceptableDiscoverRules(TSelector contract)
    {
        var registrations = _registrations.Where(x => x.CanLoad(contract));
        if (EntityExtractorDiscoverRuleApplyBehavior.OnlyFirstAcceptedRegistration == _behavior)
        {
            registrations = registrations.Take(1);
        }

        return registrations;
    }

    #endregion

    #endregion
}