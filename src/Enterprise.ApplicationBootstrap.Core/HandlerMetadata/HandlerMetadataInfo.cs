using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.ApplicationBootstrap.Core.HandlerMetadata;

/// <summary>
/// Container for extracted attributes of request handler implementation type.
/// </summary>
/// <seealso cref="HandlerMetadataStore"/>
public record HandlerMetadataInfo
{
    private readonly Dictionary<Type, object> _byType;

    /// <summary> c-tor. </summary>
    public HandlerMetadataInfo(Type handlerType)
    {
        Attributes = handlerType.GetCustomAttributes(true);
        _byType = Attributes.ToDictionary(x => x.GetType(), x => x);
    }

    /// <summary>
    /// Attributes for handler type.
    /// </summary>
    public object[] Attributes { get; }

    /// <summary>
    /// Tries to extract attribute for type <see cref="T"/> from handler metadata.
    /// Returns true and filled <paramref name="attribute"/> if attribute was found and false and empty <paramref name="attribute"/> otherwise.
    /// </summary>
    public bool TryGetAttribute<T>(out T attribute) where T : class
    {
        if (_byType.TryGetValue(typeof(T), out var val) && val != null)
        {
            attribute = (T)val;
            return true;
        }

        attribute = null;
        return false;
    }
}