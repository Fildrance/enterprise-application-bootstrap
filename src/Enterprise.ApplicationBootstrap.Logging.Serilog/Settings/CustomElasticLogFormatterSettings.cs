using Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog.Settings;

/// <summary>
/// Custom serilog formatter settings.
/// </summary>
[PublicAPI]
public class CustomElasticLogFormatterSettings
{
    public const string DefaultSectionName = "CustomElasticLogFormatterSettings";

    /// <summary>
    /// List of allowed field names, that can be logged. Will be put in structured logs nested in 'fields'.
    /// </summary>
    /// <remarks>
    /// If same name is used here and in <see cref="RootPropertyNames"/>, then field will be returned in root object.
    /// </remarks>
    [CanBeNull, ItemNotNull]
    public string[] AllowedExtraFields { get; set; }

    /// <summary>
    /// List of allowed field names, that should be logged in root object of log message object.
    /// </summary>
    /// <remarks>
    /// If same name is used here and in <see cref="AllowedExtraFields"/>, then field will be returned in root object.
    /// </remarks>
    [CanBeNull, ItemNotNull]
    public string[] RootPropertyNames { get; set; }

    /// <summary>
    /// Name of strategy class that should be used to determine a way of handling non-allowed field-names.
    /// Should be written as "[namespace].[class-name], [assembly-name-without-extension]"
    /// </summary>
    /// <remarks>
    /// List of out-of-the-box <para/>
    /// Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling.<see cref="IgnoreInvalidFieldNameHandlingStrategy"/><para/>
    /// Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling.<see cref="ExceptionThrowingInvalidFieldNameHandlingStrategy"/><para/>
    /// Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling.<see cref="CleaningInvalidFieldNameHandlingStrategy"/><para/>
    /// </remarks>
    [CanBeNull]
    public string InvalidFieldNameHandlingStrategy { get; set; }
}