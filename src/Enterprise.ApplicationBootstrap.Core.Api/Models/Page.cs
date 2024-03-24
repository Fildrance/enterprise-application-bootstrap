using System.Collections.Generic;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Api.Models;

/// <summary> Page of items. If <see cref="TotalCount"/> != <see cref="Items"/>. Length, then results are paged and only part of total results is present.</summary>
public class Page<T>
{
    /// <summary>
    /// Items in current page. 
    /// </summary>
    [NotNull]
    public IReadOnlyCollection<T> Items { get; }

    /// <summary>
    /// Total count of items that fit original request.
    /// </summary>
    public int TotalCount { get; }

    /// <summary> C-tor. </summary>
    /// <param name="items">Items in current page.</param>
    /// <param name="totalCount">Total count of items that fit original request.</param>
    public Page([NotNull] IReadOnlyCollection<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}

/// <summary>
/// Static helpers for <see cref="Page{T}"/> class.
/// </summary>
public static class Page
{
    /// <summary>
    /// Factory for creating page without directly declaring item type.
    /// </summary>
    /// <param name="items">Items in current page.</param>
    /// <param name="totalCount">Total count of items that fit original request.</param>
    /// <typeparam name="TItems">Type of items</typeparam>
    public static Page<TItems> Create<TItems>([NotNull] IReadOnlyCollection<TItems> items, int totalCount) => new(items, totalCount);
}