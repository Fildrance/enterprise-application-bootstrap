using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Context of call for swagger ui configuration.
/// </summary>
/// <param name="ServiceProvider">Application DI Container.</param>
/// <param name="SwaggerSpecPath">Path for swagger.json file that must be generated.</param>
/// <param name="LatestVersionDocumentName">Defines name to be used for latest version of api document.</param>
public record SwaggerUiConfigureContext(
    [NotNull] IServiceProvider ServiceProvider,
    [NotNull] string SwaggerSpecPath,
    [NotNull] string LatestVersionDocumentName
);