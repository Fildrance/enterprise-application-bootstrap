using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Additional selector configuration. I used to add configuration in DiscoveringRepositories.
/// </summary>
/// <typeparam name="TSelector">Type of selector.</typeparam>
/// <typeparam name="TEntity">Type of Entity.</typeparam>
public interface IAdditionalExtractConfiguration<TSelector, TEntity>
    where TEntity : class
{
    /// <summary>
    /// Adding additional configuration for selector.
    /// </summary>
    /// <param name="builder">Instance of extractor builder.</param>
    void RefineExtractConfiguration([NotNull] EntityExtractorBuilder<TSelector, TEntity> builder);
}