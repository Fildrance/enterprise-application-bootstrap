using System;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;

namespace Enterprise.ApplicationBootstrap.Core.Logging;

/// <summary>
/// Container of metadata for <see cref="LoggingBehaviour{TRequest,TResponse}"/> local caching of contract to log message converter functions.
/// </summary>
internal readonly struct ContractMetadataForLogs(object obj, HandlerMetadataInfo handlerMetadataInfo)
{
    /// <summary> Contract type (request / response type). </summary>
    public Type ContractType { get; } = obj.GetType();

    /// <summary> Metadata of handler type. </summary>
    public HandlerMetadataInfo HandlerMetadataInfo { get; } = handlerMetadataInfo;
}