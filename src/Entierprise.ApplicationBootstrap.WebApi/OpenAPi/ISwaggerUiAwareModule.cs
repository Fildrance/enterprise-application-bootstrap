using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Module for additional configuration of swagger-ui.
/// </summary>
[PublicAPI]
public interface ISwaggerUiAwareModule : IModule
{
    /// <summary>
    /// Configures swagger ui.
    /// </summary>
    /// <param name="options">Options of swagger ui.</param>
    /// <param name="context">Context of call(contains application configuration etc).</param>
    void Configure(
        [NotNull] SwaggerUIOptions options,
        [NotNull] SwaggerUiConfigureContext context
    );
}