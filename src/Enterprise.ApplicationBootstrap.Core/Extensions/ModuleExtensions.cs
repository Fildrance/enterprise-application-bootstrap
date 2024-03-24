using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Extensions;

/// <summary>
/// Extension methods for modules.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Executes <paramref name="action"/> for each module in <paramref name="moduleCollection"/>,
    /// that implements <typeparamref name="TModule"/>.
    /// </summary>
    /// <typeparam name="TModule">Filter for module type to be used in action.</typeparam>
    /// <param name="moduleCollection">List of modules.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="logger">Logger of application.</param>
    /// <param name="typeDesignation">Description of <see cref="TModule"/> type parameter to describe action being performed.</param>
    public static void ForEachOf<TModule>(
        [NotNull, ItemNotNull] this IEnumerable<IModule> moduleCollection,
        [NotNull] Action<TModule> action,
        [NotNull] ILogger logger,
        [CanBeNull] string typeDesignation = null
    ) where TModule : IModule
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var modules = moduleCollection.OfType<TModule>().ToArray();
        if (modules.Length <= 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(typeDesignation))
        {
            typeDesignation = typeof(TModule).Name;
        }

        var moduleNames = string.Join("','", modules.Select(x => x.GetType().FullName));
        logger.LogDebug("Found {modulesCount} modules, {typeDesignation}: '{modulesList}'.", modules.Length, typeDesignation, moduleNames);

        foreach (var module in modules)
        {
            logger.WrapInLogTrace<TModule>(() => action(module), module.GetType().FullName);
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> for each module in <paramref name="moduleCollection"/>,
    /// that implements <typeparamref name="TModule"/>.
    /// </summary>
    /// <typeparam name="TModule">Filter for module type to be used in action.</typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="moduleCollection">List of modules.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="logger">Logger of application.</param>
    /// <param name="typeDesignation">Description of <see cref="TModule"/> type parameter to describe action being performed.</param>
    public static IReadOnlyCollection<TResult> ForEachOf<TModule, TResult>(
        [NotNull, ItemNotNull] this IEnumerable<IModule> moduleCollection,
        [NotNull] Func<TModule, TResult> action,
        [NotNull] ILogger logger,
        [CanBeNull] string typeDesignation = null
    ) where TModule : IModule
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var modules = moduleCollection.OfType<TModule>().ToArray();
        if (modules.Length <= 0)
        {
            return Array.Empty<TResult>();
        }

        if (string.IsNullOrEmpty(typeDesignation))
        {
            typeDesignation = typeof(TModule).Name;
        }

        var moduleNames = string.Join("','", modules.Select(x => x.GetType().FullName));
        logger.LogDebug("Found {modulesCount} modules, {typeDesignation}: '{modulesList}'.", modules.Length, typeDesignation, moduleNames);

        return modules.Select(module => logger.WrapInLogTrace<TModule, TResult>(() => action(module), module.GetType().FullName))
                      .ToArray();
    }
}