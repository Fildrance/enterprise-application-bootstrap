using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Logging.Serilog.Settings;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog.Extension;

/// <summary>
/// Module that initializes <see cref="ElasticSearchFormatterConfigurationObservable"/>.
/// </summary>
internal class LogFormatterSettingsInitializingModule(
    [NotNull] IOptionsMonitor<CustomElasticLogFormatterSettings> optionsMonitor
) : BackgroundService
{
    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ElasticSearchFormatterConfigurationObservable.Init(optionsMonitor);
        return Task.CompletedTask;
    }
}