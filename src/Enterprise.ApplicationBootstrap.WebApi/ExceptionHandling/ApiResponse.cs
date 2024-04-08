using JetBrains.Annotations;
using System.Net;
using System.Text.Json.Serialization;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Base type for message of error during api call.
/// </summary>
[PublicAPI]
public class ApiErrorResponse
{
    /// <summary>
    /// http-status-code for response. Will be included into response as field too.
    /// </summary>
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Text message about error occurred.
    /// </summary>
    [CanBeNull]
    public string Message { get; set; }
}