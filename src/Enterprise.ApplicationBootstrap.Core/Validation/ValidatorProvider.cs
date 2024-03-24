using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Validation;

/// <summary>
/// Default <see cref="IValidatorProvider"/> implementation.
/// </summary>
public class ValidatorProvider : IValidatorProvider
{
    private readonly IReadOnlyCollection<IValidator> _validators;
    private readonly ConcurrentDictionary<Type, IValidator> _cache = new();

    private readonly Func<Type, IValidator> _validatorFactoryFunc;

    /// <summary> C-tor. </summary>
    public ValidatorProvider([NotNull, ItemNotNull] IEnumerable<IValidator> validators)
    {
        _validators = validators.ToArray();

        // pre-initialize delegate to remove delegate creation on each GetOrAdd Method call
        _validatorFactoryFunc = Factory;
    }

    /// <inheritdoc />
    public IValidator FindValidator(Type typeToValidate)
        => _cache.GetOrAdd(typeToValidate, _validatorFactoryFunc);

    /// <inheritdoc />
    public IValidator FindValidator<T>()
        => FindValidator(typeof(T));

    /// <summary>
    /// Creates composite validator, that is going to handle all validations.
    /// </summary>
    protected virtual IValidator CompositeFactory(IEnumerable<IValidator> validators)
        => new CompositeValidator(validators);

    private IValidator Factory(Type type)
    {
        var fittingValidators = _validators.Where(
            x => x.CanValidateInstancesOfType(type)
        ).ToArray();
        return fittingValidators.Length > 1
            ? CompositeFactory(fittingValidators)
            : fittingValidators.FirstOrDefault();
    }
}