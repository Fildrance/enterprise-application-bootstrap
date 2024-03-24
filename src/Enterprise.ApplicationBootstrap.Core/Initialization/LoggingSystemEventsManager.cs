using System;
using Enterprise.ApplicationBootstrap.Core.SystemEvents;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Initialization;

/// <summary>
/// Logging implementation of <see cref="SystemEventsManager"/>.
/// </summary>
[PublicAPI]
public class LoggingSystemEventsManager : SystemEventsManager.DefaultSystemEventsManager
{
    private readonly ILogger<LoggingSystemEventsManager> _logger;

    /// <summary> c-tor. </summary>
    public LoggingSystemEventsManager([NotNull] ILogger<LoggingSystemEventsManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public override void Complete(SystemEventName eventName)
    {
        _logger.LogInformation("Event '{systemEventName}' occurred.", eventName);
        base.Complete(eventName);
    }
}