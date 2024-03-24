using System;
using Enterprise.ApplicationBootstrap.Core.Initialization;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.SystemEvents;

/// <summary>
/// Type-marker for system events. System events are things like:<para/>
/// * end of 'Configure' call for web-application (which means DI is ready),<para/>
/// * every <see cref="IOnApplicationStartInitializable"/> returns <see cref="IOnApplicationStartInitializable.Initialized"/>
/// as True (initialization complete),<para/>
/// * application health-check returns OK,<para/>
/// * etc.
/// </summary>
public class SystemEventName : IEquatable<SystemEventName>
{
    private readonly string _value;

    /// <summary> C-tor. </summary>
    public SystemEventName([NotNull] string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        _value = value;
    }

    /// <summary> Converts to string. </summary>
    [CanBeNull]
    public static implicit operator string([CanBeNull] SystemEventName name) => name?.ToString();

    /// <summary> Converts from string. </summary>
    [CanBeNull]
    public static implicit operator SystemEventName([CanBeNull] string name) => name == null ? null : new SystemEventName(name);

    /// <inheritdoc />
    public override string ToString()
    {
        return _value;
    }

    /// <inheritdoc />
    public bool Equals(SystemEventName other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _value == other._value;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SystemEventName)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _value != null
            ? _value.GetHashCode()
            : 0;
    }
}