using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.Core.Extensions;

/// <summary>
/// Extensions for module logging.
/// </summary>
internal static class LoggerExtensions
{
    /// <summary>
    /// Wraps actions for certain interface with start/finish logs.
    /// </summary>
    /// <typeparam name="TInterface">Type of interface that will be mentioned in start/finish logs.</typeparam>
    /// <param name="logger">Logger for writing start/finish.</param>
    /// <param name="action">Action to be wrapped.</param>
    /// <param name="typeName">Name of type to be used in execution.</param>
    internal static void WrapInLogTrace<TInterface>(
        [NotNull] this ILogger logger,
        [NotNull] Action action,
        [NotNull] string typeName
    )
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var interfaceName = typeof(TInterface).FullName;
        logger.LogTrace("Starting execution {typeName} as {interfaceName}", typeName, interfaceName);
        action();
        logger.LogTrace("Finished execution {typeName} as {interfaceName}", typeName, interfaceName);
    }

    /// <summary>
    /// Wraps actions for certain interface with start/finish logs.
    /// </summary>
    /// <typeparam name="TInterface">Type of interface that will be mentioned in start/finish logs.</typeparam>
    /// <param name="logger">Logger for writing start/finish.</param>
    /// <param name="action">Action to be wrapped.</param>
    /// <param name="typeName">Name of type to be used in execution.</param>
    internal static T WrapInLogTrace<TInterface, T>(
        [NotNull] this ILogger logger,
        [NotNull] Func<T> action,
        [NotNull] string typeName
    )
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var interfaceName = typeof(TInterface).FullName;
        logger.LogTrace("Starting execution {typeName} as {interfaceName}", typeName, interfaceName);
        var result = action();
        logger.LogTrace("Finished execution {typeName} as {interfaceName}", typeName, interfaceName);
        return result;
    }

    /// <summary>
    /// Logs (in human-readable way) collection of modules and interfaces that they implement.
    /// </summary>
    internal static void LogConfigureStart(
        [NotNull] this ILogger logger,
        [NotNull] ApplicationName applicationName,
        [NotNull, ItemNotNull] IEnumerable<IModule> modules
    )
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (applicationName == null)
        {
            throw new ArgumentNullException(nameof(applicationName));
        }

        const string firstLevelSeparator = "\r\n\t * ";
        const string secondLevelSeparator = "\r\n\t\t ** ";

        var initMessage = "\r\n*********************************************************" +
                          $"\r\nStarting application '{applicationName}' with modules. " +
                          "\r\n*********************************************************";
        var moduleInfos = modules.Select(x =>
        {
            var moduleType = x.GetType();
            var implementedNames = moduleType.GetInterfaces()
                                             .Where(i => i != typeof(IModule))
                                             .Select(i => i.FullName)
                                             .ToArray();
            return new
            {
                moduleType.Name,
                moduleType.FullName,
                implementedNames
            };
        }).Select(
            x => $"{firstLevelSeparator}{x.Name}, '{x.FullName}'"
                 + (x.implementedNames.Length > 0
                     ? $", \r\n\t   implements: {secondLevelSeparator + string.Join(secondLevelSeparator, x.implementedNames)}"
                     : string.Empty
                 )
        ).ToArray();

        var modulesData = moduleInfos.Length > 0
            ? $"\r\n{string.Join("", moduleInfos)}"
            : "\r\nThere is no modules in application.";

        logger.LogInformation(initMessage + modulesData);
    }
}