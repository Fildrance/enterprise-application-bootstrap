using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary>
/// Generic type for working with simple ORM-based storage abstractions.
/// </summary>
/// <typeparam name="TEntity">Main entity type for repository. Repository might interact more then one entity type, but it is optimized for one in generic type.</typeparam>
/// <typeparam name="TId">Type of id field in <typeparamref name="TEntity"/></typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : EntityBase<TId>
{
    /// <summary> Adds record to database. </summary>
    /// <param name="entity">Entity to be saved.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Promise of saving entity.</returns>
    Task Add(TEntity entity, CancellationToken ct);

    /// <summary> Performs delete of record in database. </summary>
    /// <param name="entity">Entity to be deleted.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Promise fo saving changes.</returns>
    Task Remove(TEntity entity, CancellationToken ct);

    /// <summary> Performs update of passed entity in database. </summary>
    /// <param name="entity">Entity to be updated. </param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Promise fo saving changes. </returns>
    Task Update(TEntity entity, CancellationToken ct);

    /// <summary> Gets entity record by id (or throws if nothing to be found). </summary>
    /// <param name="id">Id to be searched.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Promise of found entity.</returns>
    Task<TEntity> GetById(TId id, CancellationToken ct);
}