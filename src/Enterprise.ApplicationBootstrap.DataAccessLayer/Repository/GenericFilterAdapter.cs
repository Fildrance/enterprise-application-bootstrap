using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> Generic implementation of filter adapter, takes care of take/skip logic. </summary>
public class GenericFilterAdapter<TFilterContract, TEntity> : IFilterAdapter<TFilterContract, TEntity>
    where TEntity : class
    where TFilterContract : FilterContractBase
{
    private readonly IFilterAdapterConcrete<TFilterContract, TEntity> _filterAdapterConcrete;
    private readonly IDbContextManager _dbContextManager;

    /// <summary> C-tor. </summary>
    /// <param name="filterAdapterConcrete"></param>
    /// <param name="dbContextManager">Context to be used in call.</param>
    public GenericFilterAdapter(IFilterAdapterConcrete<TFilterContract, TEntity> filterAdapterConcrete, IDbContextManager dbContextManager)
    {
        _filterAdapterConcrete = filterAdapterConcrete;
        _dbContextManager = dbContextManager;
    }

    /// <inheritdoc />
    public async Task<Page<TEntity>> QueryAsync(TFilterContract filter, Session session, CancellationToken ct)
    {
        var dbContext = await _dbContextManager.GetContextAsync(session, ct: ct);
        IQueryable<TEntity> query = dbContext.Set<TEntity>();

        query = await _filterAdapterConcrete.ApplyFilterAsync(query, filter, ct);

        var totalCount = await query.CountAsync(cancellationToken: ct);

        query = await _filterAdapterConcrete.ApplySortAsync(query, filter, ct);

        if (filter.Take.HasValue)
        {
            query = query.Take(filter.Take.Value);
        }

        if (filter.Skip.HasValue)
        {
            query = query.Skip(filter.Skip.Value);
        }

        var result = await query.ToArrayAsync(cancellationToken: ct);
        return Page.Create(result, totalCount);
    }
}