using System;
using System.Collections.Generic;
using System.IO;
using Asp.Versioning.ApiExplorer;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.WebApi.Modules;
using Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Default implementation of multiple major points of open-api - swagger json gen and swagger ui + endpoint metadata generation.
/// </summary>
[PublicAPI]
public class DefaultOpenApiModule([NotNull] AppInitializationContext context) : IServiceCollectionAwareModule, IAfterEndpointsAwareModule, IRouteHandlerBuilderAwareModule
{
    #region Properties

    /// <inheritdoc />
    public string ModuleIdentity => "OpenApiModule";

    /// <summary>
    /// Defines name to be used for latest version of api document.
    /// </summary>
    [NotNull]
    public virtual string LatestVersionDocumentName => "latest";

    /// <summary>
    /// Name to be used for swagger page title.
    /// </summary>
    [NotNull]
    public virtual string OpenApiInfoDocumentTitle { get; } = context.ApplicationName.Name;

    /// <summary>
    /// Marker, is 'latest' version is valid version for API.
    /// </summary>
    public virtual bool SupportingLatestVersionDocument => true;

    /// <summary>
    /// Logger.
    /// </summary>
    [NotNull]
    protected ILogger Logger { get; } = context.Logger;

    /// <summary>
    /// Path for swagger.json file. Must contain {0} as placeholder for version name.
    /// </summary>
    [NotNull]
    protected virtual string SwaggerSpecPath => "swagger/{0}/swagger.json";

    #endregion

    /// <inheritdoc />
    public void ConfigureRoutHandlers(RouteHandlerBuilder routeHandlerBuilder, ServiceRouteMetadata metadata)
    {
        routeHandlerBuilder.WithOpenApi(operation =>
        {
            operation.OperationId = metadata.MethodName;
            return operation;
        });
    }

    /// <inheritdoc />
    public virtual void Configure(MvcOptions options)
    {
        var latestDocVersionConvention = new LatestDocVersionConvention();
        options.Conventions.Add(latestDocVersionConvention);
    }

    /// <inheritdoc />
    public virtual void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(this)
                .AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddTransient(ConfigurableSwaggerOptionsFactory)
                .AddTransient(ConfigurableSwaggerUiOptionsFactory);
        Logger.LogInformation("Application will provide swagger generation functions.");
    }

    /// <inheritdoc />
    public virtual void Configure(IApplicationBuilder app)
    {
        app.UseSwagger(ConfigureSwagger);
        app.UseSwaggerUI();
        Logger.LogInformation("Swagger UI is added.");
    }

    /// <summary> Overrides OpenApi options. </summary>
    protected virtual void ConfigureSwagger([NotNull] SwaggerOptions swaggerOptions)
    {
        swaggerOptions.RouteTemplate = string.Format(SwaggerSpecPath, "{documentName}");
    }

    /// <summary> Creates configurator for swagger options. </summary>
    protected virtual IConfigureOptions<SwaggerGenOptions> ConfigurableSwaggerOptionsFactory(IServiceProvider sp)
    {
        return new ConfigurableSwaggerOptions(
            sp.GetServices<ISwaggerGenAwareModule>(),
            this,
            sp.GetRequiredService<IConfiguration>()
        );
    }

    /// <summary> Creates setup type for swagger-ui-options. </summary>
    protected virtual IConfigureOptions<SwaggerUIOptions> ConfigurableSwaggerUiOptionsFactory(IServiceProvider sp)
    {
        return new ConfigurableSwaggerUiOptions(sp, sp.GetServices<ISwaggerUiAwareModule>(), this);
    }

    /// <summary>
    /// Default type for setting up swagger-gen options.
    /// Introduced due to dependency on <see cref="IApiVersionDescriptionProvider"/>.
    /// </summary>
    public class ConfigurableSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IEnumerable<ISwaggerGenAwareModule> _swaggerGenAwareModules;
        private readonly DefaultOpenApiModule _parentModule;
        private readonly IConfiguration _configuration;

        /// <summary> c-tor. </summary>
        public ConfigurableSwaggerOptions(
            IEnumerable<ISwaggerGenAwareModule> swaggerGenAwareModules,
            DefaultOpenApiModule parentModule,
            IConfiguration configuration
        )
        {
            _swaggerGenAwareModules = swaggerGenAwareModules;
            _parentModule = parentModule;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public virtual void Configure(SwaggerGenOptions options)
        {
            var openApiInfo = new OpenApiInfo
            {
                Title = _parentModule.OpenApiInfoDocumentTitle,
                Version = _parentModule.LatestVersionDocumentName
            };
            options.SwaggerDoc(_parentModule.LatestVersionDocumentName, openApiInfo);

            options.EnableAnnotations();
            IncludeXmlComments(options);


            foreach (var swaggerGenAwareModule in _swaggerGenAwareModules)
            {
                swaggerGenAwareModule.Configure(options, _configuration);
            }
        }

        private static void IncludeXmlComments(SwaggerGenOptions options)
        {
            // includes generated xml-doc with comment text for OpenApi
            Array.ForEach(
                Directory.GetFiles(AppContext.BaseDirectory, "*.xml"),
                f => options.IncludeXmlComments(f, true)
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="modules"></param>
    /// <param name="parentModule"></param>
    public class ConfigurableSwaggerUiOptions(IServiceProvider sp, IEnumerable<ISwaggerUiAwareModule> modules, DefaultOpenApiModule parentModule)
        : IConfigureOptions<SwaggerUIOptions>
    {
        /// <inheritdoc />
        public virtual void Configure(SwaggerUIOptions options)
        {
            options.SwaggerEndpoint(
                string.Format(parentModule.SwaggerSpecPath, parentModule.LatestVersionDocumentName),
                "Current version"
            );
            options.RoutePrefix = string.Empty;

            var configureContext = new SwaggerUiConfigureContext(sp, parentModule.SwaggerSpecPath, parentModule.LatestVersionDocumentName);
            foreach (var swaggerUiAware in modules)
            {
                swaggerUiAware.Configure(options, configureContext);
            }
        }
    }
}