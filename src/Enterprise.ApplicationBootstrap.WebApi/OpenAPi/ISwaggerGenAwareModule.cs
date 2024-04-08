using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Module for additional configuration of swagger-generation.
/// </summary>
public interface ISwaggerGenAwareModule : IModule
{
    /// <summary>
    /// Configures swagger json generation options.
    /// </summary>
    /// <param name="swaggerGenOptions">Options of swagger json generator.</param>
    /// <param name="configuration">Configuration of application.</param>
    void Configure([NotNull] SwaggerGenOptions swaggerGenOptions, [NotNull] IConfiguration configuration);
}