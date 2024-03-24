using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Validation;

/// <summary>
/// Behaviour for <see cref="IMediator"/> that will validate incoming requests.
/// </summary>
public class ValidatingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IValidatorProvider _validatorProvider;
    private readonly ILogger _logger;

    /// <summary> c-tor. </summary>
    public ValidatingBehaviour(IValidatorProvider validatorProvider, ILogger<ValidatingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _validatorProvider = validatorProvider;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await EnsureIsValid(request, cancellationToken);

        return await next();
    }

    private async Task EnsureIsValid(TRequest message, CancellationToken cancellationToken)
    {
        var validator = _validatorProvider.FindValidator<TRequest>();
        if (validator == null)
        {
            return;
        }

        var validationContext = new ValidationContext<TRequest>(message);
        var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

        if (validationResult.IsValid)
        {
            return;
        }

        if (validationResult.Errors.Any(x => x.Severity == Severity.Error))
        {
            throw new ValidationException(validationResult.Errors);
        }

        _logger.LogWarning(
            "Validator error for type '{messageType}' message were found! Validation returned "
            + "only 'warning' level errors, so request will be handled. It is recommended to "
            + "check source of requests to create requests correctly. Error details: "
            + "\r\n {validationResult}.",
            message.GetType().FullName,
            validationResult.ToString()
        );
    }
}