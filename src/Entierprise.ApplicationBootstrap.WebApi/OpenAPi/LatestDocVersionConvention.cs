using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

namespace Enterprise.ApplicationBootstrap.WebApi.OpenAPi;

/// <summary>
/// Adds new routes as 'Latest' version for swagger conventions.
/// </summary>
public class LatestDocVersionConvention : IApplicationModelConvention
{
    private const string ApiVersionTemplate = "v{version:apiVersion}";
    private const char SlashChar = '/';

    /// <inheritdoc />
    public void Apply(ApplicationModel application)
    {
        foreach (var applicationController in application.Controllers)
        {
            var selectors = applicationController.Selectors;
            for (int idx = 0, initCount = selectors.Count; idx < initCount; idx++)
            {
                var originalTemplate = selectors[idx].AttributeRouteModel?.Template;
                if (originalTemplate == null)
                {
                    continue;
                }

                if (TryRemoveVersionFromTemplate(originalTemplate, out var template))
                {
                    var selectorModel = new SelectorModel
                    {
                        AttributeRouteModel = new AttributeRouteModel { Template = template }
                    };
                    selectors.Add(selectorModel);
                }
            }
        }
    }

    private static bool TryRemoveVersionFromTemplate(string template, out string result)
    {
        var pos = template.IndexOf(ApiVersionTemplate, StringComparison.InvariantCulture);
        if (pos == -1)
        {
            result = null;
            return false;
        }

        result = (template.StartsWith(SlashChar) ? SlashChar : string.Empty) +
                 template[(pos + ApiVersionTemplate.Length)..].TrimStart(SlashChar);
        return true;
    }
}