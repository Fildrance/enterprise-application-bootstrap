using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Enterprise.ApplicationBootstrap.Core.HealthChecks;

/// <summary>
/// Extension methods for working with health-checks.
/// </summary>
[PublicAPI]
public static class HealthCheckStartupExtensions
{
    /// <summary>
    /// Adds health-check into application health-checks with 'self-' prefix and passed name.
    /// This prefix is filter for checking liveness of application.
    /// </summary>
    /// <typeparam name="THealthCheck">Check implementation type (of <see cref="IHealthCheck"/>) to be added.</typeparam>
    /// <param name="builder">Builder for health-checks.</param>
    /// <param name="name">
    /// Display name for health-check. Method will add prefix <see cref="HealthCheckDefinitions.SelfHealthCheckPrefix"/> before adding it.
    /// </param>
    /// <param name="failureStatus"><see cref="HealthStatus"/>, to return in case of failure.</param>
    /// <param name="tags">Tags for health-check. Can be used for filtration.</param>
    /// <param name="timeout">Timeout for check run.</param>
    [NotNull]
    public static IHealthChecksBuilder AddSelf<THealthCheck>(
        [NotNull] this IHealthChecksBuilder builder,
        [NotNull] string name,
        [CanBeNull] HealthStatus? failureStatus = null,
        [CanBeNull, ItemNotNull] IEnumerable<string> tags = null,
        [CanBeNull] TimeSpan? timeout = null
    ) where THealthCheck : class, IHealthCheck
        => builder.Add<THealthCheck>(HealthCheckDefinitions.ToSelfCheck(name), failureStatus, tags, timeout);

    /// <summary>
    /// Adds health-check into application health-checks with 'readiness-' prefix and passed name.
    /// This prefix is filter for checking can application serve incoming requests (k8s removes service from LB on non-healthy).
    /// </summary>
    /// <typeparam name="THealthCheck">Check implementation type (of <see cref="IHealthCheck"/>) to be added.</typeparam>
    /// <param name="builder">Builder for health-checks.</param>
    /// <param name="name">
    /// Display name for health-check. Method will add prefix <see cref="HealthCheckDefinitions.ReadinessHealthCheckPrefix"/> before adding it.
    /// </param>
    /// <param name="failureStatus"><see cref="HealthStatus"/>, to return in case of failure.</param>
    /// <param name="tags">Tags for health-check. Can be used for filtration.</param>
    /// <param name="timeout">Timeout for check run.</param>
    [NotNull]
    public static IHealthChecksBuilder AddReadiness<THealthCheck>(
        [NotNull] this IHealthChecksBuilder builder,
        [NotNull] string name,
        [CanBeNull] HealthStatus? failureStatus = null,
        [CanBeNull, ItemNotNull] IEnumerable<string> tags = null,
        [CanBeNull] TimeSpan? timeout = null
    ) where THealthCheck : class, IHealthCheck
        => builder.Add<THealthCheck>(HealthCheckDefinitions.ToReadinessCheck(name), failureStatus, tags, timeout);

    /// <summary>
    /// Adds health-check into application health-checks with 'startup-' prefix and passed name.
    /// This prefix is filter for checking had application already started and running
    /// (on returning unhealthy for too long k8s considers pod creation failed).
    /// </summary>
    /// <typeparam name="THealthCheck">Check implementation type (of <see cref="IHealthCheck"/>) to be added.</typeparam>
    /// <param name="builder">Builder for health-checks.</param>
    /// <param name="name">
    /// Display name for health-check. Method will add prefix <see cref="HealthCheckDefinitions.StartupHealthCheckPrefix"/> before adding it.
    /// </param>
    /// <param name="failureStatus"><see cref="HealthStatus"/>, to return in case of failure.</param>
    /// <param name="tags">Tags for health-check. Can be used for filtration.</param>
    /// <param name="timeout">Timeout for check run.</param>
    [NotNull]
    public static IHealthChecksBuilder AddStartup<THealthCheck>(
        [NotNull] this IHealthChecksBuilder builder,
        [NotNull] string name,
        [CanBeNull] HealthStatus? failureStatus = null,
        [CanBeNull, ItemNotNull] IEnumerable<string> tags = null,
        [CanBeNull] TimeSpan? timeout = null
    ) where THealthCheck : class, IHealthCheck
        => builder.Add<THealthCheck>(HealthCheckDefinitions.ToStartupCheck(name), failureStatus, tags, timeout);

    /// <summary>
    /// Adds health-check into application health-checks with passed name.
    /// </summary>
    /// <typeparam name="THealthCheck">Check implementation type (of <see cref="IHealthCheck"/>) to be added.</typeparam>
    /// <param name="builder">Builder for health-checks.</param>
    /// <param name="name">Display name for health-check. Can be used for filtering.</param>
    /// <param name="failureStatus"><see cref="HealthStatus"/>, to return in case of failure.</param>
    /// <param name="tags">Tags for health-check. Can be used for filtration.</param>
    /// <param name="timeout">Timeout for check run.</param>
    [NotNull]
    public static IHealthChecksBuilder Add<THealthCheck>(
        [NotNull] this IHealthChecksBuilder builder,
        [NotNull] string name,
        [CanBeNull] HealthStatus? failureStatus = null,
        [CanBeNull, ItemNotNull] IEnumerable<string> tags = null,
        [CanBeNull] TimeSpan? timeout = null
    ) where THealthCheck : class, IHealthCheck
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name,
            sp => sp.GetRequiredService<THealthCheck>(),
            failureStatus,
            tags,
            timeout
        );

        builder.Services.TryAddSingleton<THealthCheck>();

        return builder.Add(healthCheckRegistration);
    }
}