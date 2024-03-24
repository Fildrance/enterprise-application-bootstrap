using System;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.Core.Logging;

/// <summary> Default implementation of <see cref="IContractToLogConverterProvider"/>. </summary>
public class ContractToLogConverterProvider(IServiceProvider serviceProvider) : IContractToLogConverterProvider
{
    /// <inheritdoc />
    public IContractToLogConverter Get(Type converterType)
    {
        var logger = serviceProvider.GetService(converterType);
        return logger as IContractToLogConverter;
    }

    /// <inheritdoc />
    public IContractToLogConverter Get(object converterKey)
    {
        var logger = serviceProvider.GetKeyedService<IContractToLogConverter>(converterKey);
        return logger;
    }
}