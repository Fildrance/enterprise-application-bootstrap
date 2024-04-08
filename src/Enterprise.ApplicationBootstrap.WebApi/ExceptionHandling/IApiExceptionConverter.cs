using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.ExceptionHandling;

/// <summary>
/// Converter for api exceptions to client messages. Works as a way to customize <see cref="ApiExceptionHandler"/>
/// exception processing for specific types.
/// </summary>
[PublicAPI]
public interface IApiExceptionConverter
{
    /// <summary>
    /// Concrete type of exception that converter can handle. No inheritance will be used.
    /// </summary>
    [NotNull]
    public Type CanHandle { get; }

    /// <summary> Converts exception to message. </summary>
    [NotNull]
    public ApiErrorResponse Handle([NotNull] Exception ex);
}