using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Api.Modules;

/// <summary> Unit of DI registration. </summary>
[PublicAPI]
public interface IModule
{
    /// <summary>
    /// Unique identifier of module. In case there will be more then one module with same identity - application will throw exception during startup.
    /// This is required to exclude destructive and unpredictable influence of multiple modules calling same configuration methods.
    /// </summary>
    [NotNull]
    string ModuleIdentity { get; }
}