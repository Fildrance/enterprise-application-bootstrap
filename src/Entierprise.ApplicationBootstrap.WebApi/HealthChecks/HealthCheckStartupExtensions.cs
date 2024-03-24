using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using Enterprise.ApplicationBootstrap.Core.HealthChecks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.WebApi.HealthChecks;

/// <summary>
/// Extension methods for working with health-checks.
/// </summary>
[PublicAPI]
public static class HealthCheckStartupExtensions
{
    /// <summary>
    /// Registers default endpoints for application health checking.
    /// </summary>
    [NotNull]
    public static IApplicationBuilder AddDefaultHealthCheckEndpoints([NotNull] this IApplicationBuilder app, ILogger logger)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        // readiness check
        app = app.SetHeathCheckEndpointForStartingWith(HealthCheckDefinitions.ReadinessHealthCheckPrefix, HealthCheckDefinitions.ReadinessEndpointPath)

                 // liveness check
                 .SetHeathCheckEndpointForStartingWith(HealthCheckDefinitions.SelfHealthCheckPrefix, HealthCheckDefinitions.LivenessEndpointPath)

                 // startup check
                 .SetHeathCheckEndpointForStartingWith(HealthCheckDefinitions.StartupHealthCheckPrefix, HealthCheckDefinitions.StartupEndpointPath)

                 // unfiltered health-check
                 .UseHealthChecks(
                     HealthCheckDefinitions.HealthCheckEndpointPath,
                     new HealthCheckOptions
                     {
                         ResponseWriter = UnescapedUnicodeUIResponseWriter.WriteHealthCheckUIResponse
                     });
        logger.LogInformation(
            "There are several health-check endpoints configured:"
            + $"\r\n * '{HealthCheckDefinitions.ReadinessEndpointPath}' - for checking if application is ready to handle requests;"
            + $"\r\n * '{HealthCheckDefinitions.LivenessEndpointPath}' - to check if application is alive and have threads to handle requests."
            + $"\r\n * '{HealthCheckDefinitions.StartupEndpointPath}' - to check if application finished initialization."
            + $"\r\n * '{HealthCheckDefinitions.HealthCheckEndpointPath}' - to check overall application status."
        );
        return app;
    }

    /// <summary>
    /// Registers endpoint for health-check with filtering by prefix.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="startingWith">Prefix, with which should <see cref="HealthCheckRegistration.Name"/> start to be used on calls to this endpoint.</param>
    /// <param name="endpoint">Endpoint relative address.</param>
    [NotNull]
    public static IApplicationBuilder SetHeathCheckEndpointForStartingWith(
        [NotNull] this IApplicationBuilder app,
        [NotNull] string startingWith,
        [NotNull] string endpoint
    )
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (string.IsNullOrWhiteSpace(startingWith))
        {
            throw new ArgumentException("Empty value", nameof(startingWith));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Empty value", nameof(endpoint));
        }

        return app.SetHeathCheckEndpointFor(r => r.Name.StartsWith(startingWith), endpoint);
    }

    /// <summary>
    /// Registers endpoint for health-check with filtering by predicate.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="predicate">Predicate for filtering health-checks to invoke.</param>
    /// <param name="endpoint">Endpoint relative address.</param>
    [NotNull]
    public static IApplicationBuilder SetHeathCheckEndpointFor(
        [NotNull] this IApplicationBuilder app,
        [NotNull] Func<HealthCheckRegistration, bool> predicate,
        [NotNull] string endpoint
    )
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Empty value", nameof(endpoint));
        }

        return app.UseHealthChecks(
            endpoint,
            new HealthCheckOptions
            {
                Predicate = predicate,
                ResponseWriter = UnescapedUnicodeUIResponseWriter.WriteHealthCheckUIResponse
            });
    }
}