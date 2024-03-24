using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Topology;

/// <summary> Invoker for trace provider modules. </summary>
public abstract class TelemetrySetupInvokerBase
{
    /// <summary>
    /// Configures telemetry functions by applying <see cref="ITraceProviderAwareModule"/> modules.
    /// </summary>
    /// <param name="services">DI Container.</param>
    /// <param name="context">Application initialization context.</param>
    /// <param name="allModules">List of all registered modules.</param>
    public void Configure(
        [NotNull] IServiceCollection services,
        [NotNull] AppInitializationContext context,
        [NotNull, ItemNotNull] IReadOnlyCollection<IModule> allModules
    )
    {
        var modules = allModules.OfType<ITraceProviderAwareModule>()
                                .ToArray();
        if (modules.Length == 0)
        {
            context.Logger.LogWarning(
                "Tracing modules were not found. There wont be any tracing set up. Tracing is vital point in observability of enterprise applications so it is  "
                + "hardly recommended to use some way of collecting and analysis telemetry information for application maintainability. "
                + $"To set up telemetry functionality add 1 or more module that implements {nameof(ITraceProviderAwareModule)} "
                + $"and registers some way to handle traces using OpenTelemetry in {nameof(ServiceProgramBase)}.GetModules."
                + "Default implementation is DefaultOpenTelemetryTracingModule from Enterprise.ApplicationBootstrap.Telemetry package."
            );
            return;
        }

        ConfigureInternal(services, context, modules);
    }

    /// <summary> Implements telemetry registration, setup and usage of modules. </summary>
    protected abstract void ConfigureInternal(
        [NotNull] IServiceCollection services,
        [NotNull] AppInitializationContext context,
        [NotNull, ItemNotNull] IReadOnlyCollection<IModule> allModules
    );
}