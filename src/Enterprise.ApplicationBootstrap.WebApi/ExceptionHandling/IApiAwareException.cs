using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Interface for exceptions that can convert themselves to <see cref="ApiErrorResponse"/> for responding to clients.
/// </summary>
/// <remarks>
/// This interface is used in <seealso cref="ApiExceptionHandler"/> and is expected to be placed on <see cref="Exception"/> types.
/// </remarks>
[PublicAPI]
public interface IApiAwareException
{
    /// <summary> Creates message for client. </summary>
    [NotNull]
    public ApiErrorResponse ToErrorResult();
}