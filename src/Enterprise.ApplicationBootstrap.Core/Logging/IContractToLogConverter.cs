namespace Enterprise.ApplicationBootstrap.Core.Logging;

/// <summary>
/// Contract object to log message converters
/// </summary>
public interface IContractToLogConverter
{
    /// <summary>
    /// Key to register default implementation of <see cref="IContractToLogConverter"/> for current application.
    /// </summary>
    public const string DefaultContractToLogConverter = "default";

    /// <summary> Generates text for log message from contract object. </summary>
    string Convert(object contract);
}