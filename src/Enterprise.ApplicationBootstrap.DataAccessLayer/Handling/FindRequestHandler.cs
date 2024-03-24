using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Discover;
using MediatR;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Handling;

/// <summary>
/// Handler for finding object by selector in repository(store) and returning it after mapping to response item.
/// </summary>
/// <typeparam name="TEntity">Type of entity in repository</typeparam>
/// <typeparam name="TRequest">Type of request that can describe a way to find entity in store.(selector).</typeparam>
/// <typeparam name="TResponse">Type of response (item).</typeparam>
/// <param name="discoverer">Discoverer that can find entity based on selector.</param>
/// <param name="mapper">Mapper for entity => item transformation.</param>
public class FindRequestHandler<TEntity, TRequest, TResponse>(
    IDiscoverer<TRequest, TEntity> discoverer,
    IMapper mapper
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TEntity : class
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var entity = await discoverer.DiscoverAsync(request, cancellationToken);
        if (entity == null)
        {
            return default;
        }

        var mapped = mapper.Map<TResponse>(entity);
        return mapped;
    }
}