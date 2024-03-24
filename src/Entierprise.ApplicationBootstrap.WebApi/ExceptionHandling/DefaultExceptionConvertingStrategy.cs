using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary> Default implementation of <see cref="IDefaultExceptionConvertingStrategy"/> (will be used if no other were found). </summary>
[PublicAPI]
public class DefaultExceptionConvertingStrategy : IDefaultExceptionConvertingStrategy
{
    internal const string StubErrorMessage = "An error occurred during request execution.";

    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<DefaultExceptionConvertingStrategy> _logger;

    /// <summary> c-tor. </summary>
    public DefaultExceptionConvertingStrategy(
        [NotNull] IHostEnvironment hostEnvironment,
        [NotNull] ILogger<DefaultExceptionConvertingStrategy> logger
    )
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public ApiErrorResponse Handle(Exception exception)
    {
        _logger.LogError(exception, "There were unexpected exception during request execution, details: {details}", exception.Message);

        return new ApiErrorResponse
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Message = _hostEnvironment.IsProduction()
                ? StubErrorMessage
                : exception.ToString()
        };
    }
}