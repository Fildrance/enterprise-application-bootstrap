using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Api.Models;

/// <summary> Base class for filter contract. Contains properties for paging. </summary>
public abstract class FilterContractBase
{
    /// <summary>
    /// Amount of records to be taken. If 'null', all records will be taken.
    /// </summary>
    [CanBeNull]
    public int? Take { get; set; }

    /// <summary>
    /// Amount of records to be skipped. If 'null', no records will be skipped.
    /// </summary>
    [CanBeNull]
    public int? Skip { get; set; }
}