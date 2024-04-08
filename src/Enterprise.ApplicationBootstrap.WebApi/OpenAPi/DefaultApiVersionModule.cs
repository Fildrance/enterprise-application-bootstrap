using System;
using System.Collections.Generic;
using System.Linq;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Module for adding http api versioning.
/// </summary>
[PublicAPI]
public class DefaultApiVersionModule : IServiceCollectionAwareModule, ISwaggerUiAwareModule
{
    /// <inheritdoc />
    public string ModuleIdentity => "ApiVersionModule";

    /// <inheritdoc />
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(SwaggerGenOptionsForOpenApiConfigureFactory)
                .AddApiVersioning(ConfigureApiVersioning)
                .AddApiExplorer(ConfigureVersionedApiExplorer);
    }

    /// <summary> Creates SwaggerGenOptionsConfigure for registering api versioning. </summary>
    protected virtual IConfigureOptions<SwaggerGenOptions> SwaggerGenOptionsForOpenApiConfigureFactory(IServiceProvider sp)
        => new SwaggerGenOptionsForOpenApiConfigure(sp.GetRequiredService<IApiVersionDescriptionProvider>(), sp.GetRequiredService<DefaultOpenApiModule>());


    /// <summary> Sets up version renderer. </summary>
    protected virtual void ConfigureVersionedApiExplorer([NotNull] ApiExplorerOptions options)
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    }

    /// <summary>
    /// Sets up control for api version analyzer and selector.
    /// </summary>
    protected virtual void ConfigureApiVersioning([NotNull] ApiVersioningOptions options)
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    }

    /// <inheritdoc />
    public void Configure(SwaggerUIOptions options, SwaggerUiConfigureContext context)
    {
        var versionProvider = context.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

        if (versionProvider.ApiVersionDescriptions.Count > 0)
        {
            foreach (var apiVersion in versionProvider.ApiVersionDescriptions.Reverse())
            {
                var versionName = apiVersion.GroupName;
                options.SwaggerEndpoint(string.Format(context.SwaggerSpecPath, versionName), $"Version - {versionName}");
            }
        }
    }

    /// <summary>
    /// Configures swagger-generation to use api versions.
    /// </summary>
    public class SwaggerGenOptionsForOpenApiConfigure : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _openApiVersionDescriptionProvider;
        private readonly DefaultOpenApiModule _openApiModule;

        /// <summary>
        /// c-tor.
        /// </summary>
        public SwaggerGenOptionsForOpenApiConfigure(
            [NotNull] IApiVersionDescriptionProvider openApiVersionDescriptionProvider,
            [NotNull] DefaultOpenApiModule openApiModule
        )
        {
            _openApiVersionDescriptionProvider = openApiVersionDescriptionProvider;
            _openApiModule = openApiModule;
        }

        /// <inheritdoc />
        public virtual void Configure(SwaggerGenOptions options)
        {
            ApiVersion maxVersion = null;
            string maxVersionDocName = null;
            var openApiDocNameToApiVersionMap = new Dictionary<string, ApiVersionDescription>();

            foreach (var description in _openApiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                var documentName = description.GroupName;
                var openApiInfo = new OpenApiInfo
                {
                    Title = _openApiModule.OpenApiInfoDocumentTitle,
                    Version = description.ApiVersion.ToString()
                };
                options.SwaggerDoc(documentName, openApiInfo);
                openApiDocNameToApiVersionMap.Add(documentName, description);

                if (maxVersion == null || description.ApiVersion > maxVersion)
                {
                    maxVersion = description.ApiVersion;
                    maxVersionDocName = documentName;
                }
            }

            options.DocInclusionPredicate(
                (docName, apiDesc) =>
                {
                    if (_openApiModule.SupportingLatestVersionDocument && docName == _openApiModule.LatestVersionDocumentName)
                    {
                        return openApiDocNameToApiVersionMap.TryGetValue(maxVersionDocName!, out var doc)
                               && doc.ApiVersion == apiDesc.GetApiVersion()
                               && !(apiDesc.RelativePath != null && apiDesc.RelativePath.Contains(maxVersionDocName));
                    }

                    return apiDesc.GroupName != null && apiDesc.GroupName.Contains(docName);
                });
        }
    }
}