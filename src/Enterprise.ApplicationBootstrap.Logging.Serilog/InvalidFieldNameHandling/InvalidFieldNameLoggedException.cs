using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling;

/// <summary>
/// Is thrown in case fields to be logged contain non-whitelisted name.
/// Whitelist of names is controlled using settings.
/// </summary>
[Serializable]
internal class InvalidFieldNameLoggedException : Exception
{
    private string _errorMessage;

    public InvalidFieldNameLoggedException([NotNull, ItemNotNull] IEnumerable<string> invalidFieldNames)
    {
        InvalidFiledNames = invalidFieldNames.ToArray();

        const string errorMessageTemplate = "There were attempt to log message with invalid field name. Invalid names - '{0}'. " +
                                            "Whitelist of acceptable names are set in 'CustomElasticLogFormatterSettings' section of appsettings.json";
        _errorMessage = string.Format(errorMessageTemplate, string.Join("','", InvalidFiledNames));
    }

    /// <inheritdoc />
    public override string Message => _errorMessage;

    /// <summary>
    /// List of invalid field names that were found.
    /// </summary>
    [NotNull, ItemNotNull]
    public string[] InvalidFiledNames { get; }
}