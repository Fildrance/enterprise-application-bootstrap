using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api.Models;

namespace Enterprise.ApplicationBootstrap.Core.BaseHandlers;

/// <summary> Base type for query consumer (that will query paged data).</summary>
public abstract class QueryRespondingRequestHandler<TIn, TOut> : IRequestHandler<TIn, Page<TOut>>
    where TIn : class, IRequest<Page<TOut>>
{
    /// <summary> Factory methods for <see cref="Page{T}"/> objects.</summary>
    protected Page<TOut> Page(IReadOnlyCollection<TOut> items, int totalCount)
    {
        return new Page<TOut>(items, totalCount);
    }

    /// <inheritdoc />
    public abstract Task<Page<TOut>> Handle(TIn request, CancellationToken cancellationToken);
}