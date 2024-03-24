using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Serilog.Events;

//WARNING! This namespace will be used in appsettings.json-files as part of strategy
//pattern to choose IInvalidFieldNameHandlingStrategy to use during runtime, so its better to never change it. See also CustomElasticLogFormatterSettings.
namespace Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling;

/// <summary>
/// Strategy for handling unexpected field names during structured logging.
/// </summary>
public interface IInvalidFieldNameHandlingStrategy
{
    /// <summary>
    /// Handles collection of properties by their names.
    /// </summary>
    /// <param name="properties"> Collection of log-able property values by their names.</param>
    /// <param name="acceptablePropertyNames">List of acceptable property names.</param>
    /// <exception cref="InvalidFieldNameLoggedException">
    /// If strategy that throws exceptions is selected, then on having 1 or more invalid property nae this exception will be thrown.
    /// </exception>
    /// <returns>List of properties to be logged without unexpected values (if proper strategy is used).</returns>
    [NotNull]
    IReadOnlyDictionary<string, LogEventPropertyValue> Handle(
        [NotNull] IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        [NotNull] IReadOnlyCollection<string> acceptablePropertyNames
    );
}

/// <summary>
/// Strategy that will ignore fields with invalid names.
/// </summary>
[UsedImplicitly]
public class IgnoreInvalidFieldNameHandlingStrategy : IInvalidFieldNameHandlingStrategy
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, LogEventPropertyValue> Handle(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        IReadOnlyCollection<string> acceptablePropertyNames
    )
    {
        return properties;
    }

    /// <summary>
    /// Default instance.
    /// </summary>
    public static IgnoreInvalidFieldNameHandlingStrategy Default { get; } = new();
}

/// <summary>
/// Strategy that will throw exception if at least one of properties is not present in whitelist.
/// </summary>
[UsedImplicitly]
public class ExceptionThrowingInvalidFieldNameHandlingStrategy : IInvalidFieldNameHandlingStrategy
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, LogEventPropertyValue> Handle(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        IReadOnlyCollection<string> acceptablePropertyNames
    )
    {
        // forming list of non-whitelisted names
        var unacceptablePropertyNames = new HashSet<string>(properties.Keys);
        unacceptablePropertyNames.ExceptWith(acceptablePropertyNames);

        if (unacceptablePropertyNames.Count > 0)
        {
            throw new InvalidFieldNameLoggedException(unacceptablePropertyNames);
        }

        return properties;
    }
}

/// <summary>
/// Strategy that removes invalid fields from list of fields to be logged.
/// </summary>
[UsedImplicitly]
public class CleaningInvalidFieldNameHandlingStrategy : IInvalidFieldNameHandlingStrategy
{
    /// <inheritdoc />
    public IReadOnlyDictionary<string, LogEventPropertyValue> Handle(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        IReadOnlyCollection<string> acceptablePropertyNames
    )
    {
        var unacceptable = properties.Keys.Except(acceptablePropertyNames);
        if (unacceptable.Any())
        {
            return properties.Where(x => acceptablePropertyNames.Contains(x.Key))
                             .ToDictionary(x => x.Key, x => x.Value);
        }

        return properties;
    }
}