using System;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Topology;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Enterprise.ApplicationBootstrap.Telemetry;

/// <summary> Default module for OpenTelemetry configuration. </summary>
[PublicAPI]
public class DefaultOpenTelemetryTracingModule : IServiceCollectionAwareModule, ITraceProviderAwareModule
{
    private readonly ApplicationName _applicationName;
    private readonly bool _isJaegerExporterEnabled;

    /// <summary> c-tor. </summary>
    /// <param name="initializationContext">Application initialization context.</param>
    /// <param name="isJaegerExporterEnabled"> <c>Jaeger</c> export settings. </param>
    /// <param name="suppressNoJaegerSettingsWarning">
    /// Suppresses warning log message which is logged in case there is no settings for Jaeger exporter found passed
    /// in c-tor or in application configuration.
    /// </param>
    public DefaultOpenTelemetryTracingModule(
        [NotNull] AppInitializationContext initializationContext,
        [CanBeNull] bool? isJaegerExporterEnabled = null,
        bool suppressNoJaegerSettingsWarning = false
    )
    {
        if (!isJaegerExporterEnabled.HasValue)
        {
            var fromConfigValue = initializationContext.Configuration["EnableJaeger"];
            _isJaegerExporterEnabled = !string.IsNullOrEmpty(fromConfigValue) && bool.TryParse(fromConfigValue, out var fromConfigBool) && fromConfigBool;
        }
        else
        {
            _isJaegerExporterEnabled = isJaegerExporterEnabled.Value;
        }

        if (_isJaegerExporterEnabled && string.IsNullOrWhiteSpace(initializationContext.Configuration["OTLP_ENDPOINT_URL"]))
        {
            throw new ArgumentException(
                $"Settings {nameof(_isJaegerExporterEnabled)} are invalid - flag '{nameof(_isJaegerExporterEnabled)}' "
                + $"is 'true', but 'OTLP_ENDPOINT_URL' is empty."
            );
        }

        _applicationName = initializationContext.ApplicationName;
        Logger = initializationContext.Logger;
        SuppressNoJaegerSettingsWarning = suppressNoJaegerSettingsWarning;
    }

    /// <inheritdoc />
    public string ModuleIdentity => "OpenTelemetryModule";

    /// <summary>
    /// Suppresses warning log message which is logged in case there is no settings for Jaeger exporter found passed
    /// in c-tor or in application configuration.
    /// </summary>
    protected bool SuppressNoJaegerSettingsWarning { get; }

    /// <summary>
    /// Logger that can be used in module.
    /// </summary>
    [NotNull]
    protected ILogger Logger { get; }

    /// <inheritdoc />
    public virtual void Configure(TracerProviderBuilder builder)
    {
        // tracing in HttpClient calls
        builder.AddHttpClientInstrumentation(); // todo: bind to AddHttpClient ???

        builder.SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                           .AddService(_applicationName)
        );

        if (!_isJaegerExporterEnabled && !SuppressNoJaegerSettingsWarning)
        {
            const string message =
                "Application includes telemetry module, but there are not settings for jaeger exporter. "
                + "Without exporting data tracing can be complicated, so it is recommended to set exporter up."
                + "To setup exporter you can either:"
                + $"\r\n * add to c-tor call of {nameof(DefaultOpenTelemetryTracingModule)} following code: "
                + "- isJaegerExporterEnabled: true\r\n" +
                "and then add to appsettings.json of executable project (or in any other way that is registered as configuration provider) "
                + "\"OTLP_ENDPOINT_URL\" : \"<your-jaeger-collector-address>:<jaeger-collector-port>\";"
                + "\r\n * add 'OTLP_ENDPOINT_URL' and 'EnableJaeger' values to env variables with corresponding values;";
            Logger.LogWarning(message);
        }

        if (_isJaegerExporterEnabled)
        {
            builder.AddOtlpExporter();
            Logger.LogInformation("Jaeger exporter is enabled.");
        }
    }

    /// <inheritdoc />
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        // registration of trace manager OpenTelemetry (as alternative to creating ActivitySource)
        services.AddSingleton(
            sp => sp.GetRequiredService<TracerProvider>()
                    .GetTracer(_applicationName)
        );
    }
}