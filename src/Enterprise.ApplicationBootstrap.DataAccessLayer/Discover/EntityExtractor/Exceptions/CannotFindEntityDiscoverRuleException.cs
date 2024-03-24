using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor.Exceptions;

/// <summary>
/// Exception that occurred due to all discover rules for selector cannot accept presented selector.
/// </summary>
public class CannotFindEntityDiscoverRuleException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="CannotFindEntityDiscoverRuleException" /> class.</summary>
    public CannotFindEntityDiscoverRuleException(object selector) : base("Failed to find object. No discover rule is matching for such definition.")
    {
        Selector = selector;
    }

    /// <summary>
    /// Selector that caused exception.
    /// </summary>
    [PublicAPI]
    public object Selector { get; }
}