using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Container for metadata of route handler that uses mediator-based services.
/// </summary>
public class ServiceRouteMetadata([NotNull] string calledMethodName)
{
    /// <summary>
    /// Name of called service method.
    /// </summary>
    [NotNull]
    public string MethodName => calledMethodName;
}