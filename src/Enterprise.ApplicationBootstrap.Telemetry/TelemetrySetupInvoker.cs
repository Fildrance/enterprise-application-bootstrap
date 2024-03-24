using System.Collections.Generic;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Extensions;
using Enterprise.ApplicationBootstrap.Core.Topology;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.Telemetry;

/// <summary> Implementation of <see cref="TelemetrySetupInvokerBase"/> that registers OpenTelemetry modules. </summary>
[PublicAPI]
public class TelemetrySetupInvoker : TelemetrySetupInvokerBase
{
    /// <inheritdoc />
    protected override void ConfigureInternal(IServiceCollection services, AppInitializationContext context, IReadOnlyCollection<IModule> allModules)
    {
        // registration of OpenTelemetry features
        services.AddOpenTelemetry()
                .WithTracing(
                    builder =>
                    {
                        allModules.ForEachOf<ITraceProviderAwareModule>(
                            x => x.Configure(builder),
                            context.Logger
                        );
                    }
                );
    }
}