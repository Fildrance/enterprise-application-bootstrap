using System;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules;

/// <summary>
/// Module that is executed after application container configuration is finished.
/// </summary>
public interface IServiceProviderAwareModule : IModule
{
    /// <summary>
    /// Executes logic, dependent on <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="sp">Ready-to-use root application service provider.</param>
    public void Configure([NotNull] IServiceProvider sp);
}