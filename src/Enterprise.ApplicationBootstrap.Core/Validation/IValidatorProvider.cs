using System;
using FluentValidation;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Validation;

/// <summary>
/// Provider for validation objects from FluentValidation.
/// </summary>
public interface IValidatorProvider
{
    /// <summary>
    /// Provides validator object that can validate passed message type.
    /// If there are more then 1 fitting validator, then it will be packed into <see cref="CompositeValidator"/>.
    /// </summary>
    /// <param name="typeToValidate">Type to be validated.</param>
    /// <returns>Validator implementation for requested type, <see cref="CompositeValidator"/> or null.</returns>
    [CanBeNull]
    IValidator FindValidator([NotNull] Type typeToValidate);

    /// <summary>
    /// Provides validator object that can validate passed message type.
    /// If there are more then 1 fitting validator, then it will be packed into <see cref="CompositeValidator"/>.
    /// </summary>
    /// <typeparam name="T">Type to be validated.</typeparam>
    /// <returns>Validator implementation for requested type, <see cref="CompositeValidator"/> or null.</returns>
    [CanBeNull]
    IValidator FindValidator<T>();
}