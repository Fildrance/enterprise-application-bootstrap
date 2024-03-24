using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.WebApi.HealthChecks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Enterprise.ApplicationBootstrap.WebApi;

/// <summary>
/// Helper for WebApplication initialization using modules.
/// </summary>
[PublicAPI]
public abstract class HttpServiceProgramBase : ServiceProgramBase
{
    /// <summary>
    /// Builds application and executes modules.
    /// </summary>
    /// <param name="builder">Builder for web application.</param>
    /// <param name="commandLineArgs">arguments from command line of application launch.</param>
    /// <returns><see cref="WebApplication"/> ready to start.</returns>
    protected WebApplication BuildApplication([NotNull] WebApplicationBuilder builder, [NotNull, ItemNotNull] string[] commandLineArgs)
    {
        RefineBuilder(builder.Host, commandLineArgs);

        SetupHostBuilder(builder.Host);

        SetupWebHostBuilder(builder.WebHost);

        var application = builder.Build();

        var applicationServices = application.Services;
        var context = applicationServices
            .GetRequiredService<AppInitializationContext>();
        var modules = applicationServices
                      .GetRequiredService<IEnumerable<IModule>>()
                      .ToArray();

        ExecuteServiceProviderAwareModules(applicationServices, context, modules);

        ConfigureWebApplicationInternal(application, context, modules);

        return application;
    }

    /// <summary>
    /// re-configures IHost.
    /// </summary>
    protected virtual void SetupHostBuilder([NotNull] IHostBuilder hostBuilder)
    {
    }

    /// <summary>
    /// re-configures IWebHost.
    /// </summary>
    protected virtual void SetupWebHostBuilder([NotNull] IWebHostBuilder webHostBuilder)
    {
    }

    /// <summary>
    /// Configures application request processing pipeline.
    /// </summary>
    /// <param name="app">Builder for application request handling pipeline.</param>
    /// <param name="context">Application initialization context.</param>
    /// <param name="modules">List of added modules.</param>
    protected virtual void ConfigureApplication(
        [NotNull] IApplicationBuilder app,
        [NotNull] AppInitializationContext context,
        [NotNull, ItemNotNull] IReadOnlyCollection<IModule> modules
    )
    {
    }

    private void ConfigureWebApplicationInternal(
        WebApplication app,
        AppInitializationContext context,
        IReadOnlyCollection<IModule> modules
    )
    {
        context.Logger.LogInformation("Starting to configure request handling pipeline.");
        try
        {
            ConfigureApplication(app, context, modules);
            AddHealthCheck(app, context.Logger);
        }
        catch (Exception e)
        {
            context.Logger.LogCritical(e, "There was unhandled error during request handling pipeline configuration.");
            throw;
        }

        context.Logger.LogInformation("Finished to configure request handling pipeline.");
    }

    private static void AddHealthCheck(IApplicationBuilder app, ILogger logger)
    {
        app.AddDefaultHealthCheckEndpoints(logger);
    }
}