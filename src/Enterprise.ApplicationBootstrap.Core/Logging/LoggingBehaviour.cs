using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.Core.Logging.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Logging;

file static class LocalCache
{
    public static readonly ConcurrentDictionary<Type, ILogger> Loggers = new();
    public static readonly ConcurrentDictionary<ContractMetadataForLogs, Func<object, string>> ToLogTransformer = new();
}

/// <summary>
/// Logging behaviour for <see cref="IMediator"/>.
/// </summary>
/// <remarks> Constant calls will allocate memory as overloads such as
/// <see cref="LoggerExtensions.LogWarning(ILogger,EventId,System.Exception?,string?,object?[])"/> use 'params' and object[].
/// So for low alloc or zero-alloc this should not be used and source-gen version of logging should be used.
/// </remarks>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IContractToLogConverterProvider _contractToLogConverterProvider;
    private readonly IContractToLogConverter _defaultConverter;
    private readonly JsonSerializerOptions _options;
    private readonly Func<Type, ILogger> _valueFactory;
    private readonly HandlerMetadataInfo _handlerMetadata;
    private readonly Func<ContractMetadataForLogs, Func<object, string>> _contractConverterFactory;

    /// <summary> c-tor. </summary>
    public LoggingBehaviour(
        ILoggerFactory loggerFactory,
        IHandlerMetadataStore handlerMetadataStore,
        IContractToLogConverterProvider contractToLogConverterProvider,
        [FromKeyedServices(IContractToLogConverter.DefaultContractToLogConverter)]
        IContractToLogConverter defaultConverter,
        JsonSerializerOptions options = null
    )
    {
        _loggerFactory = loggerFactory;
        _contractToLogConverterProvider = contractToLogConverterProvider;
        _defaultConverter = defaultConverter;
        _options = options;
        _handlerMetadata = handlerMetadataStore.Get<TRequest, TResponse>();

        loggerFactory.CreateLogger(nameof(LoggingBehaviour<TRequest, TResponse>))
                     .LogDebug("Created LoggingBehaviour with '{type}' default contract to log converter.", _defaultConverter.GetType().FullName);

        _valueFactory = GetLogger;
        _contractConverterFactory = ContractConverterFactory;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var key = typeof(TRequest);
        var logger = LocalCache.Loggers.GetOrAdd(key, _valueFactory);
        TResponse response;
        try
        {
            logger.LogDebug("Starting handling '{typeToHandle}'.", key.FullName);


            if (logger.IsEnabled(LogLevel.Trace))
            {
                var contractMetadataForLogs = new ContractMetadataForLogs(request, _handlerMetadata);
                var converter = LocalCache.ToLogTransformer.GetOrAdd(contractMetadataForLogs, _contractConverterFactory);

                var asString = converter(request);
                logger.LogTrace("Using as argument => '{contract}'.", asString);
            }

            response = await next();
            logger.LogDebug("Finished handling '{typeToHandle}'.", key.FullName);
            if (logger.IsEnabled(LogLevel.Trace))
            {
                var contractMetadataForLogs = new ContractMetadataForLogs(response, _handlerMetadata);
                var converter = LocalCache.ToLogTransformer.GetOrAdd(contractMetadataForLogs, _contractConverterFactory);

                var asString = converter(response);
                logger.LogTrace("Returned result argument => '{contract}'.", asString);
            }
        }
        catch
        {
            logger.LogDebug("Failed handling '{typeToHandle}'.", key.FullName);
            throw;
        }

        return response;
    }


    private Func<object, string> ContractConverterFactory(ContractMetadataForLogs metadata)
    {
        var converterForType = GetConverterForType(metadata.ContractType)
                               ?? GetConverterForType(metadata.HandlerMetadataInfo)
                               ?? _defaultConverter.Convert;

        return converterForType;
    }

    private Func<object, string> GetConverterForType(HandlerMetadataInfo handlerMetadata)
    {
        return GetConverterForType(handlerMetadata.Attributes);
    }

    private Func<object, string> GetConverterForType(Type contractType)
    {
        // use cached for request
        var customAttributes = contractType.GetCustomAttributes();
        var attributes = customAttributes.ToArray();
        if (attributes.Any(x => x is LogAsJsonAttribute))
        {
            return contract => JsonSerializer.Serialize(contract, contractType, _options);
        }

        return GetConverterForType(attributes);
    }

    private Func<object, string> GetConverterForType(IReadOnlyCollection<object> attributes)
    {
        var customLoggerByTypeAttribute = attributes.OfType<UseConverterToLogByTypeAttribute>()
                                                    .FirstOrDefault();
        if (customLoggerByTypeAttribute != null)
        {
            var contractToLogConverter = _contractToLogConverterProvider.Get(customLoggerByTypeAttribute.ConverterType);
            {
                return contractToLogConverter.Convert;
            }
        }

        var customLoggerByKeyAttribute = attributes.OfType<UseConverterToLogByKeyAttribute>()
                                                   .FirstOrDefault();
        if (customLoggerByKeyAttribute != null)
        {
            var contractToLogConverter = _contractToLogConverterProvider.Get(customLoggerByKeyAttribute.ConverterKey);
            {
                return contractToLogConverter.Convert;
            }
        }

        return o => o.ToString();
    }

    private ILogger GetLogger(Type type) => _loggerFactory.CreateLogger(type);
}