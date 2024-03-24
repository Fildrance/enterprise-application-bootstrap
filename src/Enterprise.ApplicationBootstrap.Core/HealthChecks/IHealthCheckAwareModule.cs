using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ApplicationBootstrap.Core.HealthChecks;

/// <summary>
/// Infrastructure module for health-check registration.
/// </summary>
[PublicAPI]
public interface IHealthCheckAwareModule : IModule
{
    /// <summary> Sets up health-checks. </summary>
    /// <param name="builder">Builder for health-check setup.</param>
    public void Configure([NotNull] IHealthChecksBuilder builder);
}