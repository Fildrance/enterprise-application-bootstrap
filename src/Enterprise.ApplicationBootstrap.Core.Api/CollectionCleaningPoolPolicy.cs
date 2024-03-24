using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;

namespace Enterprise.ApplicationBootstrap.Core.Api;

/// <summary>
/// Pooling policy that cleans and trims Dictionary on return to pool.
/// </summary>
public class CollectionCleaningPoolPolicy : IPooledObjectPolicy<Dictionary<string, object>>
{
    private const int MaxNonTrimmedSessionDictionarySize = 50;

    /// <inheritdoc />
    public Dictionary<string, object> Create() => new();

    /// <inheritdoc />
    public bool Return(Dictionary<string, object> obj)
    {
        var needsTrim = obj.Count > MaxNonTrimmedSessionDictionarySize;
        obj.Clear();
        if (needsTrim)
        {
            obj.TrimExcess(8);
        }

        return true;
    }
}