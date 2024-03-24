using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Default implementation of <see cref="IApiAwareException"/>.
/// </summary>
/// <remarks>
/// If default behaviour needs to be modified, register custom implementation of <see cref="IDefaultExceptionConvertingStrategy"/>.<para/>
/// If exception type to be handled can be modified - implement <see cref="IApiAwareException"/>.<para/>
/// If exception type is unreachable - implement custom <see cref="IApiExceptionConverter"/>.
/// </remarks>
public class ApiExceptionHandler : IApiExceptionHandler
{
    internal const string UnexpectedDirectControllerCallMessage = $"There were no {nameof(IExceptionHandlerFeature)} initialized to handle exception!";

    private readonly IDefaultExceptionConvertingStrategy _defaultExceptionHandlingStrategy;
    private readonly Dictionary<Type, IApiExceptionConverter> _apiExceptionHandlers;
    private readonly ILogger<ApiExceptionHandler> _logger;

    /// <summary> c-tor. </summary>
    public ApiExceptionHandler(
        [NotNull] IDefaultExceptionConvertingStrategy defaultExceptionHandlingStrategy,
        [NotNull] IEnumerable<IApiExceptionConverter> apiExceptionHandlers,
        [NotNull] ILogger<ApiExceptionHandler> logger
    )
    {
        _defaultExceptionHandlingStrategy = defaultExceptionHandlingStrategy;
        _apiExceptionHandlers = apiExceptionHandlers.ToDictionary(x => x.CanHandle);
        _logger = logger;
    }

    /// <inheritdoc />
    public ApiErrorResponse HandleException(HttpContext httpContext)
    {
        var context = httpContext.Features.Get<IExceptionHandlerFeature>();
        var ex = context?.Error;
        if (null == ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new ApiErrorResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = UnexpectedDirectControllerCallMessage
            };
        }

        var result = GetResponse(ex);
        httpContext.Response.StatusCode = (int)result.StatusCode;
        return result;
    }

    private ApiErrorResponse GetResponse(Exception e)
    {
        var exceptionType = e.GetType();
        if (_apiExceptionHandlers.TryGetValue(exceptionType, out var handler))
        {
            return handler.Handle(e);
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (e is not IApiAwareException apiAwareException)
        {
            return _defaultExceptionHandlingStrategy.Handle(e);
        }

        var result = apiAwareException.ToErrorResult();
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning(e, e.Message);
        }
        else
        {
            _logger.LogError(e, e.Message);
        }

        return result;

    }
}