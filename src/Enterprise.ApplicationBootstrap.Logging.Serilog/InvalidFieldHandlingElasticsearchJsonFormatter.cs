using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Enterprise.ApplicationBootstrap.Logging.Serilog.InvalidFieldNameHandling;
using Enterprise.ApplicationBootstrap.Logging.Serilog.Settings;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog;

/// <summary>
/// Serilog formatter, that logs messages in json format for ElasticSearch. Most of implementation is inherited from
/// <see cref="ElasticsearchJsonFormatter"/>, it also can whitelist fields that are logged.
/// </summary>
/// <remarks>
/// Reason behind creation of this formatter are: <para/>
/// * ElasticSearch is performing much worse when there is a ton of indexed fields in indexes, so its better to control them.<para/>
/// * Logstash filtering is an option to filter unneeded fields, but it is hard to keep hundreds of application
/// configurations (as each application can have 5-10 fields that require indexing)<para/>
/// It is, however, important to mention that filtering introduces allocations in some scenarios, which degrades performance.
/// </remarks>
public class InvalidFieldHandlingElasticSearchJsonFormatter : ElasticsearchJsonFormatter, IObserver<CustomElasticLogFormatterSettings>
{
    private static readonly string[] DefaultAcceptablePropertyNames = { "SourceContext" };
    private static readonly string[] DefaultRootPropertyNames = { "Application", "SpanId", "ParentId", "TraceId" };

    private static readonly HashSet<string> ExceptionBasePropertyNames = new(
        typeof(Exception).GetFields(BindingFlags.Instance | BindingFlags.Public)
                         .Select(x => x.Name)
                         .Concat(
                             typeof(Exception).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                              .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                                              .Select(x => x.Name)
                         )
    );

    private static readonly ConcurrentDictionary<Type, Dictionary<string, Func<Exception, object>>> ExceptionPropertiesProviders = new();

    private readonly bool _inlineFields;
    private HashSet<string> _acceptablePropertyNames;
    private HashSet<string> _rootPropertyNames;
    private IInvalidFieldNameHandlingStrategy _invalidFieldNameHandlingStrategy;
    private readonly IDisposable _observableSubscription;

    private IInvalidFieldNameHandlingStrategy InvalidFieldNameHandlingStrategy
        => _invalidFieldNameHandlingStrategy ?? IgnoreInvalidFieldNameHandlingStrategy.Default;

    /// <inheritdoc cref="ElasticsearchJsonFormatter" />
    public InvalidFieldHandlingElasticSearchJsonFormatter(
        bool omitEnclosingObject = false,
        string closingDelimiter = null,
        bool renderMessage = true,
        IFormatProvider formatProvider = null,
        ISerializer serializer = null,
        bool inlineFields = false,
        bool renderMessageTemplate = true,
        bool formatStackTraceAsArray = false
    ) : base(
        omitEnclosingObject, closingDelimiter, renderMessage, formatProvider,
        serializer, inlineFields, renderMessageTemplate, formatStackTraceAsArray
    )
    {
        _inlineFields = inlineFields;
        _observableSubscription = ElasticSearchFormatterConfigurationObservable.Instance.Subscribe(this);
    }

    ~InvalidFieldHandlingElasticSearchJsonFormatter()
    {
        _observableSubscription.Dispose();
    }

    /// <inheritdoc />
    public void OnCompleted()
    {
        // no-op
    }

    /// <inheritdoc />
    public void OnError(Exception error)
    {
        // no-op
    }

    /// <summary>
    /// On every settings change - re-create whitelist of field names and re-initialize handling strategy.
    /// </summary>
    /// <param name="value">Link to new (or changed) serilog formatter settings.</param>
    public void OnNext(CustomElasticLogFormatterSettings value)
    {
        _rootPropertyNames = new HashSet<string>(
            value?.RootPropertyNames?.Concat(DefaultRootPropertyNames)
            ?? DefaultRootPropertyNames
        );

        _acceptablePropertyNames = new HashSet<string>(
            value?.AllowedExtraFields?.Concat(DefaultAcceptablePropertyNames)
            ?? DefaultAcceptablePropertyNames
        );
        _acceptablePropertyNames.ExceptWith(_rootPropertyNames);

        _invalidFieldNameHandlingStrategy =
            !string.IsNullOrWhiteSpace(value?.InvalidFieldNameHandlingStrategy)
            && Type.GetType(value!.InvalidFieldNameHandlingStrategy!, false) is { } type
                ? (IInvalidFieldNameHandlingStrategy)Activator.CreateInstance(type)
                : IgnoreInvalidFieldNameHandlingStrategy.Default;
    }

    /// <inheritdoc />
    protected override void WriteProperties(
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output
    )
    {
        var precedingDelimiter = ",";
        Dictionary<string, LogEventPropertyValue> withoutRoot = null;
        foreach (var rootProperty in properties.Where(x => _rootPropertyNames!.Contains(x.Key)))
        {
            WriteJsonProperty(rootProperty.Key, rootProperty.Value, ref precedingDelimiter, output);
            withoutRoot ??= properties.ToDictionary(x => x.Key, x => x.Value);
            withoutRoot.Remove(rootProperty.Key);
        }

        properties = InvalidFieldNameHandlingStrategy.Handle(
            withoutRoot ?? properties,
            _acceptablePropertyNames!
        );

        if (!_inlineFields)
        {
            output.Write(",\"{0}\":{{", "fields");
        }
        else
        {
            output.Write(",");
        }

        precedingDelimiter = "";
        foreach (var property in properties)
        {
            WriteJsonProperty(property.Key, property.Value, ref precedingDelimiter, output);
        }

        if (!_inlineFields)
        {
            output.Write("}");
        }
    }

    /// <inheritdoc />
    protected override void WriteException(Exception exception, ref string delim, TextWriter output)
    {
        output.Write(delim);
        output.Write("\"exceptions\":[");
        delim = "";
        WriteExceptionSerializationInfo(exception, ref delim, output, 0);
        output.Write("]");
    }

    // ReSharper disable once IdentifierTypo
    private void WriteExceptionSerializationInfo(
        Exception exception,
        ref string delim,
        TextWriter output,
        int depth
    )
    {
        for (; exception != null && depth <= 20; exception = exception.InnerException, depth++)
        {
            output.Write(delim);
            output.Write("{");
            delim = "";
            WriteSingleException(exception, ref delim, output, depth);
            WriteExceptionDataDictionary(exception, ",", output);
            WriteExceptionCustomProperties(exception, ",", output);

            output.Write("}");
            delim = ",";
        }
    }

    // ReSharper disable once IdentifierTypo
    private void WriteExceptionDataDictionary(Exception exception, string delim, TextWriter output)
    {
        if (exception.Data is not { Count: > 0 } data)
        {
            return;
        }

        WriteJsonDictionaryProperty(
            nameof(Exception.Data),
            data.Cast<DictionaryEntry>().Select(x => (x.Key, x.Value)),
            delim,
            output
        );
    }

    // ReSharper disable once IdentifierTypo
    private void WriteExceptionCustomProperties(Exception exception, string delim, TextWriter output)
    {
        var typePropertyInfos = ExceptionPropertiesProviders.GetOrAdd(
            exception.GetType(),
            GetExceptionPropertiesOrFields
        );
        if (typePropertyInfos.Count == 0)
        {
            return;
        }

        var propertyWrites = typePropertyInfos.Aggregate(
            new List<Action<string, TextWriter>>(),
            (actions, property) =>
            {
                var value = property.Value.Invoke(exception);
                if (value != null)
                {
                    var name = property.Key;

                    if (value is IEnumerable collection and not string)
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        var e = collection.GetEnumerator();
                        if (e.MoveNext())
                        {
                            // ReSharper disable once PossibleMultipleEnumeration
                            actions.Add((d, o) => WriteJsonArrayProperty(name, collection, ref d, o));
                        }

                        if (e is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    else
                    {
                        actions.Add((d, o) => WriteJsonProperty(name, value, ref d, o));
                    }
                }

                return actions;
            });

        if (propertyWrites.Count > 0)
        {
            output.Write(delim);
            output.Write("\"custom\":{");
            delim = "";
            foreach (var action in propertyWrites)
            {
                action(delim, output);
                delim = ",";
            }

            output.Write("}");
        }
    }

    private static Dictionary<string, Func<Exception, object>> GetExceptionPropertiesOrFields(Type exceptionType)
        => exceptionType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .Select(x => (x.Name, ValueProvider: (Func<Exception, object>)x.GetValue))
                        .Concat(
                            exceptionType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                         .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                                         .Select(x => (x.Name, ValueProvider: (Func<Exception, object>)x.GetValue))
                        )
                        .Where(x => !ExceptionBasePropertyNames.Contains(x.Name))
                        .ToDictionary(x => x.Name, x => x.ValueProvider);

    // ReSharper disable once IdentifierTypo
    private void WriteJsonDictionaryProperty<TKey, TValue>(
        string name,
        IEnumerable<(TKey Key, TValue Value)> pairs,
        string delim,
        TextWriter output
    )
    {
        output.Write($"{delim}\"{name}\":");
        var elements = pairs.ToDictionary(
            x => new ScalarValue(x.Key),
            x => (LogEventPropertyValue)new ScalarValue(x.Value)
        );
        WriteDictionary(elements, output);
    }
}