using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using OpenTelemetry.Trace;

namespace Enterprise.ApplicationBootstrap.Core.Topology;

/// <summary> Module for setting up telemetry. </summary>
[PublicAPI]
public interface ITraceProviderAwareModule : IModule
{
    /// <summary> Configures up tracing in application. </summary>
    void Configure([NotNull] TracerProviderBuilder builder);
}