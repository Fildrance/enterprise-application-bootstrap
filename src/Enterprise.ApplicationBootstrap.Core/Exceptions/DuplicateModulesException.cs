using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Exceptions;

/// <summary>
/// Thrown in case multiple modules with same identity were found during application startup.
/// </summary>
[PublicAPI]
public class DuplicateModulesException : Exception
{
    /// <inheritdoc />
    public DuplicateModulesException(
        [NotNull, ItemNotNull] IEnumerable<IGrouping<string, IModule>> modulesGroupedByIdentity
    ) : base(GetMessage(modulesGroupedByIdentity))
    {
    }

    private static string GetMessage(IEnumerable<IGrouping<string, IModule>> modulesGroupedByIdentity)
    {
        var info = "Found multiple modules with duplicate identity registered: ";
        foreach (var duplicateModule in modulesGroupedByIdentity)
        {
            var types = duplicateModule.Select(x => x.GetType().FullName);
            info += $"\r\n\tduplicate of modules - types '{string.Join("', '", types)}' have identity = '{duplicateModule.Key}'";
        }

        info +=
            "\r\nproperty 'Identity' is marker of unique-ness for deriving types - "
            + "modules does not support usage of multiple instance of base implementation in one application, "
            + "having duplicates registered usually is sign of logical error. "
            + "This may be result of deriving type from module and using it in the same time as parent module "
            + "instead using just derived one and calling base implementation of methods for all methods that are suitable. "
            + "To mend this problem try to remove registration of parent modules from list of duplicates, or remove inheritance "
            + "in modules.";

        return info;
    }
}