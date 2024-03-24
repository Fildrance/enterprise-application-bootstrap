using System;

namespace Enterprise.ApplicationBootstrap.Core.Logging;

/// <summary>
/// Provider for contract object to log message converters.
/// </summary>
public interface IContractToLogConverterProvider
{
    /// <summary> Returns converter based of requested type (acquired from ServiceProvider by passed type). </summary>
    IContractToLogConverter Get(Type converterType);

    /// <summary>
    /// Returns converter (by acquiring from ServiceProvider as keyed service
    /// using <see cref="converterKey"/> and <see cref="IContractToLogConverter"/> interface).
    /// </summary>
    IContractToLogConverter Get(object converterKey);
}