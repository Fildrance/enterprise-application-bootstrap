using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Api.Models;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> 
/// Adapter for database query that gets entities based on filter contract. 
/// As it uses paging, it is required to be called multiple times to get all records, if they won't be presented on single page.
/// </summary>
public interface IFilterAdapter<in TFilterContract, TEntity>
{
    /// <summary> Applies filter/sort logic to db to get entities based on filter contract. </summary>
    /// <param name="filter">Contract that contains data, based on which selection and sorting of entities should be executed.</param>
    /// <param name="session">Session during which request is made.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    /// <returns>Promise of paged entities list. </returns>
    Task<Page<TEntity>> QueryAsync(TFilterContract filter, Session session, CancellationToken ct);
}