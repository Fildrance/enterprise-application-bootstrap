using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enterprise.ApplicationBootstrap.WebApi.Serialization;

/// <summary>
/// Converter for <see cref="TimeSpan"/> into <c>JSON</c>-string and backwards.
/// </summary>
public class JsonStringTimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value == null ? default : TimeSpan.Parse(value);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}