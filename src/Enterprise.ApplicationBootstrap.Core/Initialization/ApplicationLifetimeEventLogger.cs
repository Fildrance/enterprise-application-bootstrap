using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Initialization;

/// <summary>
/// Service that registers logs on Host lifetime event.
/// </summary>
[PublicAPI]
public class ApplicationLifetimeEventLogger : IOnApplicationStartInitializable
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary> c-tor. </summary>
    public ApplicationLifetimeEventLogger(
        [NotNull] IHostApplicationLifetime lifetime,
        [NotNull] ILoggerFactory loggerFactory
    )
    {
        _lifetime = lifetime;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public Task Initialize(CancellationToken ct)
    {
        var logger = _loggerFactory.CreateLogger("ApplicationLifetimeLogger");
        _lifetime.ApplicationStarted.Register(GetLogCallback("Application started."));
        _lifetime.ApplicationStopping.Register(GetLogCallback("Application is shutting down..."));
        _lifetime.ApplicationStopped.Register(GetLogCallback("Application stopped."));

        Initialized = true;
        return Task.CompletedTask;

        Action GetLogCallback(string message) => () => logger.LogInformation(message);
    }

    /// <inheritdoc />
    public bool Initialized { get; private set; }
}