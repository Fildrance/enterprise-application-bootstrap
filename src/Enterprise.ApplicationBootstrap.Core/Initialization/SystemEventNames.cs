using Enterprise.ApplicationBootstrap.Core.SystemEvents;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Initialization;

/// <summary>
/// <see cref="SystemEventName"/> for well-known events.
/// </summary>
[PublicAPI]
public static class SystemEventNames
{
    /// <summary>
    /// Call to 'Configure' of application is finished - DI container configuration complete.
    /// </summary>
    [NotNull] public static readonly SystemEventName ConfigurationComplete = nameof(ConfigurationComplete);

    /// <summary>
    /// Health-check returned 'healthy' for the first time since the app-start.
    /// </summary>
    [NotNull] public static readonly SystemEventName HealthCheckHealthy = nameof(HealthCheckHealthy);

    /// <summary>
    /// Every initializable service (<see cref="IOnApplicationStartInitializable"/>) completed initialization.
    /// </summary>
    [NotNull] public static readonly SystemEventName InitializableServicesInitialized = nameof(InitializableServicesInitialized);
}