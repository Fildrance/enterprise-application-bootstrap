using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog.Extension;

/// <summary>
/// Extension methods for <see cref="IHostBuilder"/>.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures log provider to use serilog and configures settings for <see cref="InvalidFieldHandlingElasticSearchJsonFormatter"/>.
    /// </summary>
    /// <param name="hostBuilder">DI container.</param>
    /// <param name="configureLoggerConfiguration">
    /// Delegate for logs configuration. If null - will try to read from <see cref="HostBuilderContext.Configuration"/>.
    /// </param>
    [PublicAPI, NotNull]
    public static IHostBuilder UseCustomSerilog(
        [NotNull] this IHostBuilder hostBuilder,
        [CanBeNull] Action<LoggerConfiguration, HostBuilderContext> configureLoggerConfiguration = null
    )
    {
        hostBuilder = hostBuilder.UseSerilog(
            (context, configuration) =>
            {
                if (configureLoggerConfiguration == null)
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                }
                else
                {
                    configureLoggerConfiguration(configuration, context);
                }
            });

        return hostBuilder.ConfigureServices(
            (builderContext, services) => services.AddCustomSerilogFormatter(builderContext.Configuration)
        );
    }
}