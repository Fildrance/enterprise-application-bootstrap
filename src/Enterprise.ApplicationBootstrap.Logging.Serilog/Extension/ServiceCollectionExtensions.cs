using Enterprise.ApplicationBootstrap.Logging.Serilog.Settings;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog.Extension;

/// <summary> <see cref="IServiceCollection"/> extensions for serilog logging configuration. </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures application to use serilog with integrated filter of fields that can be logged
    /// as separate fields <see cref="InvalidFieldHandlingElasticSearchJsonFormatter"/>.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="configuration">Application configuration.</param>
    [PublicAPI, NotNull]
    public static IServiceCollection AddCustomSerilogFormatter(
        [NotNull] this IServiceCollection services,
        [NotNull] IConfiguration configuration
    )
    {
        services.Configure<CustomElasticLogFormatterSettings>(
            configuration.GetSection(CustomElasticLogFormatterSettings.DefaultSectionName));
        services.AddHostedService<LogFormatterSettingsInitializingModule>();
        return services;
    }

    /// <summary>
    /// Configures log provider to use serilog and configures settings for <see cref="InvalidFieldHandlingElasticSearchJsonFormatter"/>.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="loggerConfiguration">Configures serilog logging. If null - will try to read from <see cref="configuration"/>.</param>
    [PublicAPI, NotNull]
    public static IServiceCollection AddCustomSerilog(
        [NotNull] this IServiceCollection services,
        [NotNull] IConfiguration configuration,
        [CanBeNull] LoggerConfiguration loggerConfiguration = null
    )
    {
        loggerConfiguration ??= new LoggerConfiguration()
                                .ReadFrom.Configuration(configuration);
        var logger = loggerConfiguration.CreateLogger();

        return services.AddCustomSerilogFormatter(configuration)
                       .AddSingleton<ILoggerProvider>(new SerilogLoggerProvider(logger));
    }
}