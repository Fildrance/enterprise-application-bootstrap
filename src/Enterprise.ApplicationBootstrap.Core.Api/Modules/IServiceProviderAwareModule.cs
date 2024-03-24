using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Api.Modules;

/// <summary> Module for actions with service provider. Is executed after DI container is configured. </summary>
[PublicAPI]
public interface IServiceProviderAwareModule : IModule
{
    /// <summary>
    /// Executes calls on <see cref="IServiceProvider"/> after application configuration is finished.
    /// </summary>
    public void Configure([NotNull] IServiceProvider sp);
}