using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Validation;

/// <summary>
/// Implements <see cref="IValidator{T}"/> that is composed of 2 or more validators and invokes them sequentially.
/// </summary>
/// <remarks> Cannot create <see cref="IValidatorDescriptor"/>. </remarks>
public class CompositeValidator : IValidator
{
    private readonly IReadOnlyCollection<IValidator> _validators;

    /// <summary> c-tor </summary>
    public CompositeValidator([NotNull, ItemNotNull] IEnumerable<IValidator> validators)
    {
        _validators = (validators ?? throw new ArgumentNullException(nameof(validators))).ToArray();
    }

    /// <inheritdoc />
    [NotNull]
    public ValidationResult Validate([NotNull] IValidationContext context)
    {
        var results = _validators.Select(x => x.Validate(context))
                                 .ToArray();

        return new ValidationResult(results.SelectMany(x => x.Errors).Distinct());
    }

    /// <inheritdoc />
    [NotNull]
    public async Task<ValidationResult> ValidateAsync([NotNull] IValidationContext context, CancellationToken cancellation = new CancellationToken())
    {
        var tasks = _validators.Select(x => x.ValidateAsync(context, cancellation))
                               .ToArray();
        var results = await Task.WhenAll(tasks);

        return new ValidationResult(results.SelectMany(x => x.Errors).Distinct());
    }

    /// <inheritdoc />
    public IValidatorDescriptor CreateDescriptor()
    {
        // this validator is was not designed for generation of rule set on front-end side.
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool CanValidateInstancesOfType([NotNull] Type type)
    {
        return _validators.All(x => x.CanValidateInstancesOfType(type));
    }
}