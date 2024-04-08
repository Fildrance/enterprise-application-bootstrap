using JetBrains.Annotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Handler for exceptions requests.
/// </summary>
public interface IApiExceptionHandler
{
    /// <summary>
    /// Handler for api exceptions.
    /// </summary>
    /// <param name="httpContext">
    /// Http context during which exception occurred. Should contain initialized
    /// <see cref="IExceptionHandlerFeature"/> for method to work properly.
    /// </param>
    /// <returns>Response object to return.</returns>
    [NotNull]
    ApiErrorResponse HandleException([NotNull] HttpContext httpContext);
}