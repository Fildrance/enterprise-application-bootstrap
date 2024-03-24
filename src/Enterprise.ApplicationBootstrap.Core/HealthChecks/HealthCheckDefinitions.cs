using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.HealthChecks;

/// <summary>
/// Constants and helpers for health-checks.
/// </summary>
[PublicAPI]
public static class HealthCheckDefinitions
{
    /// <summary> Endpoint path for readiness probe. Filters health-checks for k8s probe.</summary>
    public static readonly string ReadinessEndpointPath = "/readiness";

    /// <summary> Endpoint path for liveness probe. Filters health-checks for k8s probe. </summary>
    public static readonly string LivenessEndpointPath = "/liveness";

    /// <summary> Endpoint path for startup probe. Filters health-checks for k8s probe. </summary>
    public static readonly string StartupEndpointPath = "/startup";

    /// <summary>
    /// Endpoint for health-check - overall application status.
    /// Returns all checks without filtration, can be used to monitor app status (without binding to k8s probes).
    /// </summary>
    public static readonly string HealthCheckEndpointPath = "/health-check";

    /// <summary> Adds 'startup-' prefix for health-check name, so it could be filtered. </summary>
    public static string ToStartupCheck([NotNull] string healthCheckName)
        => StartupHealthCheckPrefix + WithoutExtraDash(healthCheckName);

    /// <summary> Adds 'readiness-' prefix for health-check name, so it could be filtered. </summary>
    public static string ToReadinessCheck([NotNull] string healthCheckName)
        => ReadinessHealthCheckPrefix + WithoutExtraDash(healthCheckName);

    /// <summary> Adds 'self-' prefix for health-check name, so it could be filtered. </summary>
    public static string ToSelfCheck([NotNull] string healthCheckName)
        => SelfHealthCheckPrefix + WithoutExtraDash(healthCheckName);

    /// <summary>
    /// Health-check prefix for checks that display that application is overall alive (for liveness probe).
    /// </summary>
    public static readonly string SelfHealthCheckPrefix = "self-";

    /// <summary> Health-check prefix for check that displays if application is started up. </summary>
    public static readonly string StartupHealthCheckPrefix = "startup-";

    /// <summary> Health-check prefix for checks that displays application readiness to process requests.  </summary>
    public static readonly string ReadinessHealthCheckPrefix = "readiness-";

    private static readonly char[] TrimChars = { '-', ' ', '\r', '\n' };

    private static string WithoutExtraDash([NotNull] string healthCheckName) 
        => healthCheckName.TrimStart(TrimChars);
}