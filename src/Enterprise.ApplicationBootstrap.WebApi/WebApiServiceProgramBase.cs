using Enterprise.ApplicationBootstrap.Core;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Extensions;
using Enterprise.ApplicationBootstrap.WebApi.Modules;
using Microsoft.Extensions.Logging;
using Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

namespace Enterprise.ApplicationBootstrap.WebApi;

/// <summary>
/// Basic implementation of utility for WebApplication initialization that will be using WebApi.
/// </summary>
[PublicAPI]
public abstract class WebApiServiceProgramBase : HttpServiceProgramBase
{
    /// <summary>
    /// Name of CORS policy to be used.
    /// </summary>
    public const string PolicyName = "AllowAll";

    /// <summary>
    /// Marker, if application should use HTTPS (and as a result - https redirect).
    /// </summary>
    protected bool UseHttps;

    /// <summary> c-tor. </summary>
    /// <param name="useHttps"> Should application use https? </param>
    protected WebApiServiceProgramBase(bool useHttps = false)
    {
        UseHttps = useHttps;
    }

    #region ConfigureApplication override

    /// <inheritdoc />
    protected override void ConfigureApplication(
        IApplicationBuilder app,
        AppInitializationContext context,
        IReadOnlyCollection<IModule> modules
    )
    {
        var logger = context.Logger;

        // error handling
        app.UseExceptionHandler(exAppBuilder => exAppBuilder.Run(RequestExceptionHandler));

        logger.LogInformation(
            $"Error handling was added to request handling pipeline. Failed requests will be handled by {nameof(ApiExceptionHandler)}."
        );
        app.UseCors(PolicyName);
        logger.LogInformation("CORS-policy was added to request handling pipeline.'");

        if (UseHttps)
        {
            app.UseHttpsRedirection();
            logger.LogInformation("Application is configured to use https redirect.");
        }

        BeforeRoutingInternal(app, modules, context.Logger);
        app.UseRouting();
        logger.LogInformation("Routing was added to request handling pipeline.'");
        AfterRoutingInternal(app, modules, context.Logger);

        BeforeEndpointsInternal(app, modules, context.Logger);
        app.UseEndpoints(
            endpoints =>
            {
                logger.LogInformation("Mapping for controllers was added to request handling pipeline.");

                ConfigureEndpointsInternal(endpoints, modules, context.Logger);

                logger.LogInformation("Endpoints configuration was added to request handling pipeline.'");
            }
        );
        AfterEndpointsInternal(app, modules, context.Logger);
    }

    // ReSharper disable SuspiciousTypeConversion.Global
    private void BeforeRoutingInternal(IApplicationBuilder app, IEnumerable<IModule> modules, ILogger logger)
    {
        modules.ForEachOf<IBeforeRoutingAwareModule>(
            x => x.Configure(app),
            logger
        );
    }

    private void AfterRoutingInternal(IApplicationBuilder app, IEnumerable<IModule> modules, ILogger logger)
    {
        modules.ForEachOf<IAfterRoutingAwareModule>(
            x => x.Configure(app),
            logger
        );
    }

    private void BeforeEndpointsInternal(IApplicationBuilder app, IEnumerable<IModule> modules, ILogger logger)
    {
        modules.ForEachOf<IBeforeEndpointsAwareModule>(
            x => x.Configure(app),
            logger
        );
    }

    private IReadOnlyCollection<IEnumerable<IEndpointRouteBuilderAggregator>> ConfigureEndpointRoutesInternal(
        IEndpointRouteBuilder endpointRouteBuilder,
        IReadOnlyCollection<IModule> modules,
        ILogger logger
    )
    {
        return modules.ForEachOf<EndpointRouteConfigureAwareModuleBase, IEnumerable<IEndpointRouteBuilderAggregator>>(
            x => x.GetEndpointRouteBuilders(endpointRouteBuilder),
            logger
        );
    }

    private void ConfigureEndpointsInternal(IEndpointRouteBuilder endpoints, IReadOnlyCollection<IModule> modules, ILogger logger)
    {
        modules.ForEachOf<IEndpointsConfigureAwareModule>(
            x => x.Configure(endpoints),
            logger
        );

        var endpointRouteAggregates = ConfigureEndpointRoutesInternal(endpoints, modules, logger)
            .SelectMany(x => x)
            .ToArray();

        modules.ForEachOf<IRouteHandlerBuilderAwareModule>(module =>
        {
            foreach (var aggregate in endpointRouteAggregates)
            {
                aggregate.Predefine(module.ConfigureRoutHandlers);
            }
        }, logger);

        logger.LogInformation($"{nameof(IRouteHandlerBuilderAwareModule)} is executed on each endpoint-route.");

        foreach (var endpointRouteBuilderAggregator in endpointRouteAggregates)
        {
            endpointRouteBuilderAggregator.Build(endpoints);
        }

        logger.LogInformation($"Finished building registered {nameof(IEndpointRouteBuilderAggregator)}.");
    }

    private void AfterEndpointsInternal(IApplicationBuilder app, IEnumerable<IModule> modules, ILogger logger)
    {
        modules.ForEachOf<IAfterEndpointsAwareModule>(
            x => x.Configure(app),
            logger
        );
    }

    // ReSharper restore SuspiciousTypeConversion.Global

    #endregion

    #region WebApi related

    /// <summary> Handler for exceptions, happened during http requests handling. </summary>
    protected virtual Task RequestExceptionHandler(HttpContext httpContext)
    {
        var helper = httpContext.RequestServices.GetRequiredService<IApiExceptionHandler>();
        var result = helper.HandleException(httpContext);
        httpContext.Response.StatusCode = (int)result.StatusCode;
        var serializerOptions = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();
        serializerOptions.Value.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        var serialized = JsonSerializer.Serialize(result, result.GetType(), serializerOptions.Value.JsonSerializerOptions);
        return httpContext.Response.WriteAsync(serialized);
    }

    /// <inheritdoc />
    protected sealed override IReadOnlyCollection<IModule> GetModules(AppInitializationContext context)
    {
        var modules = GetWebApiModules(context);
        var moduleList = new List<IModule>(modules)
        {
            new WebApiRegistrationModule(this, modules, context)
        };
        return moduleList.AsReadOnly();
    }

    /// <inheritdoc />
    protected override void RegisterModulesAsServices(
        IServiceCollection services,
        IReadOnlyCollection<IModule> modules,
        AppInitializationContext appInitializationContext
    )
    {
        base.RegisterModulesAsServices(services, modules, appInitializationContext);
        services.AddSingleton(this);
    }

    /// <inheritdoc cref="GetModules" />
    [NotNull, ItemNotNull]
    protected abstract IReadOnlyCollection<IModule> GetWebApiModules([NotNull] AppInitializationContext context);

    /// <summary>
    /// CORS settings.
    /// </summary>
    /// <remarks>
    /// Is called during <see cref="WebApiRegistrationModule.Configure"/>.
    /// </remarks>
    /// <param name="options">Options object for setting up CORS.</param>
    /// <param name="context">Application info.</param>
    protected virtual void AddCors([NotNull] CorsOptions options, [NotNull] AppInitializationContext context)
    {
        context.Logger.LogDebug(
            $"Added CORS policies {nameof(CorsPolicyBuilder.AllowAnyOrigin)}, "
            + $"{nameof(CorsPolicyBuilder.AllowAnyHeader)}, "
            + $"{nameof(CorsPolicyBuilder.AllowAnyMethod)} under policy name '{PolicyName}'."
        );
        options.AddPolicy(
            PolicyName,
            builder => builder.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod()
        );
    }

    /// <summary>
    /// Internal module for registering web-api related functionality of <see cref="WebApiServiceProgramBase"/>.
    /// </summary>
    private class WebApiRegistrationModule(
            WebApiServiceProgramBase programBase,
            IEnumerable<IModule> modules,
            AppInitializationContext context
        )
        : IServiceCollectionAwareModule
    {
        /// <inheritdoc />
        public string ModuleIdentity => "CoreWebApiModule";

        /// <inheritdoc />
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            var logger = context.Logger;
            logger.LogInformation("Starting configurations for WebApi.");

            services.AddCors(options => programBase.AddCors(options, context));
            // todo - create SEAM for overrides
            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                var serializerOptions = options.JsonSerializerOptions;
                serializerOptions.Converters.Add(new JsonStringEnumConverter());
                serializerOptions.PropertyNamingPolicy = null;
            });

            services.AddSingleton<IDefaultExceptionConvertingStrategy, DefaultExceptionConvertingStrategy>();
            services.AddSingleton<IApiExceptionHandler, ApiExceptionHandler>();
        }
    }

    #endregion
}