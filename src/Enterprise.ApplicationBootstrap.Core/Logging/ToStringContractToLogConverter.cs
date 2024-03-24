using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Logging;

/// <summary>
/// Converter for transformation from contract to log message, tht will simply call ToString on contract.
/// </summary>
[PublicAPI]
public sealed class ToStringContractToLogConverter : IContractToLogConverter
{
    /// <inheritdoc />
    public string Convert(object contract) => contract.ToString();
}