using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core;

/// <summary>
/// Class-container for name of application. 
/// </summary>
[PublicAPI]
public sealed class ApplicationName
{
    /// <summary>
    /// Name of application.
    /// </summary>
    public string Name { get; }

    private ApplicationName([NotNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
    }

    /// <summary> Extracts string name from <see cref="ApplicationName"/>. </summary>
    [NotNull]
    public static implicit operator string([NotNull] ApplicationName applicationName)
        => applicationName == null
            ? throw new ArgumentNullException(nameof(applicationName))
            : applicationName.Name;

    /// <summary>
    /// Forms application name based on application entry point.
    /// </summary>
    /// <param name="entryPointType"> Type from application entry point assembly </param>
    [NotNull]
    public static ApplicationName BuildApplicationName(Type entryPointType)
    {
        return BuildApplicationName(entryPointType.Assembly);
    }

    /// <summary>
    /// Extracts application name from Assembly by convention.
    /// </summary>
    [NotNull]
    public static ApplicationName BuildApplicationName(Assembly entryPointAssembly)
    {
        var entryPointNameSpaceName = entryPointAssembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(entryPointNameSpaceName))
        {
            const string message = "Failed to determine name for application. "
                                   + "Application name is required by multiple systems (metrics, logs, etc) "
                                   + "that are critical for 'healthy' application exploitation.";
            throw new InvalidOperationException(message);
        }

        return new ApplicationName(entryPointNameSpaceName);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}