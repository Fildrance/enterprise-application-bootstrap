using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.WebApi.Serialization;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;

// ReSharper disable InconsistentNaming

namespace Enterprise.ApplicationBootstrap.WebApi.HealthChecks;

/// <summary>
/// Custom <see cref="UIResponseWriter"/> for serialization of health-check without usage of unicode_escape.
/// </summary>
internal static class UnescapedUnicodeUIResponseWriter
{
    private static readonly byte[] EmptyResponse = { 123, 125 };

    private static readonly Lazy<JsonSerializerOptions> Options = new(CreateJsonOptions);

    /// <summary>
    /// Converts health-check results to string representation and writes it to <see cref="HttpContext.Response"/>.
    /// </summary>
    public static async Task WriteHealthCheckUIResponse(
        HttpContext httpContext,
        HealthReport report
    )
    {
        if (report == null)
        {
            await httpContext.Response.BodyWriter.WriteAsync((ReadOnlyMemory<byte>)EmptyResponse);
        }
        else
        {
            httpContext.Response.ContentType = "application/json";
            var from = UIHealthReport.CreateFrom(report);
            using (var responseStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(responseStream, from, Options.Value);
                await httpContext.Response.BodyWriter.WriteAsync((ReadOnlyMemory<byte>)responseStream.ToArray());
            }
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.Converters.Add(new JsonStringTimeSpanConverter());

        return jsonOptions;
    }
}