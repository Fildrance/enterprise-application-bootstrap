using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor.Exceptions;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover;

/// <summary> Base class for repository with discover capabilities.</summary>
/// <typeparam name="TSelector">Type of selector to be used in discovers.</typeparam>
/// <typeparam name="TEntity">Type of entity.</typeparam>
/// <typeparam name="TId">Type of id of entity.</typeparam>
public abstract class DiscoveringRepositoryBase<TSelector, TEntity, TId>
    : RepositoryBase<TEntity, TId>,
        IDiscoverer<TSelector, TEntity>
    where TEntity : EntityBase<TId>
{
    #region [Fields]

    private IEntityExtractor<TSelector, TEntity> _entityExtractor;
    private readonly IEnumerable<IAdditionalExtractConfiguration<TSelector, TEntity>> _additionalConfigurations;

    #endregion

    #region [c-tor]

    /// <summary>
    /// Creates repository.
    /// </summary>
    protected DiscoveringRepositoryBase(
        IDbContextManager dbContextManager,
        IEnumerable<IAdditionalExtractConfiguration<TSelector, TEntity>> additionalConfigurations
    ) : base(dbContextManager)
    {
        _additionalConfigurations = additionalConfigurations;
    }

    #endregion

    #region IDiscoverer<TSelector,TEntity> implementation

    /// <inheritdoc />
    public Task<TEntity> DiscoverAsync(TSelector selector, CancellationToken ct)
    {
        var set = GetSet();
        return EntityExtractor.ExtractAsync(selector, set, ct);
    }

    #endregion

    #region [Public]

    #region [Public methods]

    /// <summary>
    /// Sets configured <see cref="EntityExtractor"/> for repository.
    /// </summary>
    public virtual void ConfigureExtractor()
    {
        var entityExtractorBuilder = new EntityExtractorBuilder<TSelector, TEntity>();

        DoConfigureExtractor(entityExtractorBuilder);

        foreach (var additionalConfiguration in _additionalConfigurations)
        {
            additionalConfiguration.RefineExtractConfiguration(entityExtractorBuilder);
        }

        EntityExtractor = entityExtractorBuilder.Build();
    }

    #endregion

    #endregion

    #region [Protected]

    #region [Protected properties]

    /// <summary>
    /// Entity Extractor that is used to discover entities.
    /// </summary>
    /// <exception cref="EntityExtractorNotConfiguredException">
    /// Is thrown in case of setting Entity extractor to null. This can be due to ConfigureExtractor was not called,
    /// or it was called but it is replaced in your implementation with invalid code that desn't set EntityExtractor value.
    /// </exception>
    protected IEntityExtractor<TSelector, TEntity> EntityExtractor
    {
        set => _entityExtractor = value;
        get
        {
            if (_entityExtractor == null)
            {
                throw new EntityExtractorNotConfiguredException();
            }

            return _entityExtractor;
        }
    }

    #endregion

    #region [Protected methods]

    /// <summary>
    /// Configures builder for entity extractor.
    /// </summary>
    protected abstract void DoConfigureExtractor(EntityExtractorBuilder<TSelector, TEntity> builder);

    #endregion

    #endregion
}