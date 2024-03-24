using JetBrains.Annotations;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core;

/// <summary>
/// Container for objects, used often during application initialization.
/// </summary>
[PublicAPI]
public sealed class AppInitializationContext
{
    /// <summary> C-tor. </summary>
    public AppInitializationContext(
        [NotNull] ApplicationName applicationName,
        [NotNull] IHostEnvironment hostEnvironment,
        [NotNull] IConfiguration configuration,
        [NotNull] ILogger logger
    )
    {
        ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary> Name of application. </summary>
    [NotNull]
    public ApplicationName ApplicationName { get; }

    /// <summary> Environment information. </summary>
    [NotNull]
    public IHostEnvironment HostEnvironment { get; }

    /// <summary> Application configuration. </summary>
    [NotNull]
    public IConfiguration Configuration { get; }

    /// <summary> Application logger. </summary>
    [NotNull]
    public ILogger Logger { get; }
}