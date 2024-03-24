using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Converter of exceptions to messages for users. Is used by <seealso cref="ApiExceptionHandler"/> if
/// no specific converter (<seealso cref="IApiExceptionConverter"/>) found for exception type.
/// </summary>
[PublicAPI]
public interface IDefaultExceptionConvertingStrategy
{
    /// <summary> Coverts exception to message for api client. </summary>
    [NotNull]
    ApiErrorResponse Handle([NotNull] Exception exception);
}