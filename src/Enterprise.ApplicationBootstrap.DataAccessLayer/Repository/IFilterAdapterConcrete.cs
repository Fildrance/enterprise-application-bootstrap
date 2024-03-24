using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> Inner concrete filter adapter that contains logic of filtering and sorting for designated entity type.</summary>
public interface IFilterAdapterConcrete<in TFilterContract, TEntity>
{
    /// <summary> Applies filtering based on filter contract data, then returns query. </summary>
    Task<IQueryable<TEntity>> ApplyFilterAsync(IQueryable<TEntity> query, TFilterContract filterContract, CancellationToken ct);
    /// <summary> Applies sorting based on filter contract data, then returns query. </summary>
    Task<IQueryable<TEntity>> ApplySortAsync(IQueryable<TEntity> query, TFilterContract filterContract, CancellationToken ct);
}