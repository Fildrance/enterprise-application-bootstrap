using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.ObjectPool;

namespace Enterprise.ApplicationBootstrap.Core.Api;

/// <summary>
/// Async-local session data store. Session can be context for web-request, message handling in MQ, job execution in Quartz etc.
/// Sessions are scoping so when new is created - old one will lie underneath it.
/// </summary>
[PublicAPI]
public class Session : IDisposable, IAsyncDisposable
{
    // ReSharper disable once InconsistentNaming
    private static readonly AsyncLocal<Session> _current = new();

    private readonly ObjectPool<Dictionary<string, object>> _objectPool;

    private readonly Session _nested;

    private Dictionary<string, object> _data;

    #region c-tor

    /// <summary>
    /// c-tor for stubs.
    /// </summary>
    private Session()
    {
        _data = new();
        _objectPool = new DefaultObjectPool<Dictionary<string, object>>(new CollectionCleaningPoolPolicy());
        Root = this;
    }

    /// <summary>
    /// c-tor. Stores session using nesting.
    /// </summary>
    /// <param name="nested">Session to nest into new one.</param>
    public Session([NotNull] Session nested) : this(nested._objectPool)
    {
        _nested = nested ?? throw new ArgumentNullException(nameof(nested));
        Root = nested.Root;
    }

    /// <summary>
    /// c-tor.
    /// </summary>
    /// <param name="objectPool">Pool of data store objects.</param>
    public Session([NotNull] ObjectPool<Dictionary<string, object>> objectPool)
    {
        _objectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
        _data = objectPool.Get();
        Root = this;
    }

    #endregion

    #region props

    /// <summary> Current context. </summary>
    [CanBeNull]
    public static Session Current
    {
        get => _current.Value;
        private set => _current.Value = value;
    }

    /// <summary>
    /// Root session object (original, first one in chain, with no other parent session).
    /// </summary>
    public Session Root { get; private set; }

    /// <summary>
    /// Marker that this session is root session (with no parent session).
    /// </summary>
    public bool IsRoot => Root == this;

    #endregion

    #region API

    /// <summary>
    /// Gets value from session store. Extracts value from current session, if none found - tries to retrieve from nested session. When no nested session found - returns default(T).
    /// </summary>
    /// <typeparam name="T">Type of item to be returned.</typeparam>
    /// <param name="key">Key under which session data to be retrieved is stored.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="key"/> is null or empty / whitespace.</exception>
    [CanBeNull]
    public virtual T Get<T>([NotNull] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_data.TryGetValue(key, out var value))
        {
            if (Equals(value, Unit.Value))
            {
                return default;
            }

            return (T)value;
        }

        return _nested == null
            ? default
            : _nested.Get<T>(key);
    }

    /// <summary>
    /// Gets value from session store. Extracts value from current session, if none found - tries to retrieve from nested session. When no nested session found - returns default(T).
    /// </summary>
    /// <typeparam name="T">Type of item to be returned.</typeparam>
    /// <param name="key">Key under which session data to be retrieved is stored.</param>
    /// <param name="value">Found value of default(T) if there were none found.</param>
    /// <exception cref="ArgumentNullException">if <paramref name="key"/> is null or empty / whitespace.</exception>
    /// <returns>True if value were found, false otherwise.</returns>
    public virtual bool TryGet<T>([NotNull] string key, [CanBeNull] out T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_data.TryGetValue(key, out var val))
        {
            value = Equals(val, Unit.Value)
                ? default
                : (T)val;

            return true;
        }

        if (_nested == null)
        {
            value = default;
            return false;
        }

        return _nested.TryGet(key, out value);
    }

    /// <summary>
    /// Gets value from session store, with session inside which value was saved.
    /// Extracts value from current session, if none found - tries to retrieve from nested session.
    /// When no nested session found - returns default(T).
    /// </summary>
    /// <typeparam name="T">Type of item to be returned.</typeparam>
    /// <param name="key">Key under which session data to be retrieved is stored.</param>
    /// <exception cref="ArgumentNullException">if <paramref name="key"/> is null or empty / whitespace.</exception>
    /// <returns>Tuple with value found (or default(T)) and session in which value was found (or null if no value were found).</returns>
    public virtual ValueWithSessionContainer<T> ExtractWithLayer<T>([NotNull] string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_data.TryGetValue(key, out var value))
        {
            if (Equals(value, Unit.Value))
            {
                return new ValueWithSessionContainer<T>(default, this);
            }

            return new ValueWithSessionContainer<T>((T)value, this);
        }

        return _nested?.ExtractWithLayer<T>(key)
               ?? default;
    }

    /// <summary>
    /// Sets value for passed key in current session. If value is null - <see cref="Unit"/> will be placed into store.
    /// </summary>
    /// <typeparam name="T">Type of value to be set.</typeparam>
    /// <param name="key">Key for value to be stored.</param>
    /// <param name="value">Value to be stored.</param>
    public virtual void Set<T>([NotNull] string key, [CanBeNull] T value)
    {
        _data[key] = Equals(value, null)
            ? Unit.Value
            : value;
    }

    #endregion

    #region ExecuteInSession

    public static TResult ExecuteInSession<TResult>(Func<TResult> func, ObjectPool<Dictionary<string, object>> objectPool)
    {
        TResult result;
        var prev = Current;
        try
        {
            EnsureSession(objectPool, prev);

            result = func();
        }
        finally
        {
            var toDispose = Current;
            Current = prev;
            toDispose.Dispose();
        }

        return result;
    }

    public static async Task<TResult> ExecuteInSession<TResult>(Func<Task<TResult>> func, ObjectPool<Dictionary<string, object>> objectPool)
    {
        TResult result;
        var prev = Current;
        try
        {
            EnsureSession(objectPool, prev);

            result = await func();
        }
        finally
        {
            var toDispose = Current;
            Current = prev;
            await toDispose.DisposeAsync();
        }

        return result;
    }

    public static void ExecuteInSession(Action func, ObjectPool<Dictionary<string, object>> objectPool)
    {
        var prev = Current;
        try
        {
            EnsureSession(objectPool, prev);

            func();
        }
        finally
        {
            var toDispose = Current;
            Current = prev;
            toDispose.Dispose();
        }
    }

    public static async Task ExecuteInSession(Func<Task> func, ObjectPool<Dictionary<string, object>> objectPool)
    {
        var prev = Current;
        try
        {
            EnsureSession(objectPool, prev);

            await func();
        }
        finally
        {
            var toDispose = Current;
            Current = prev;
            await toDispose.DisposeAsync();
        }
    }

    #endregion


    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeInternal();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeInternal().GetAwaiter()
                         .GetResult();
    }

    private async Task DisposeInternal()
    {
        if (_data == null)
        {
            return;
        }

        foreach (var kvp in _data)
        {
            switch (kvp.Value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _objectPool?.Return(_data);
        _data = null;
    }

    private static void EnsureSession(ObjectPool<Dictionary<string, object>> objectPool, Session prev)
    {
        Current = prev == null
            ? new Session(objectPool)
            : new Session(prev);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="session"></param>
    /// <typeparam name="T"></typeparam>
    public struct ValueWithSessionContainer<T>([CanBeNull] T value, [CanBeNull] Session session)
    {
        /// <summary>
        /// Deconstruct into pieces
        /// </summary>
        /// <param name="outValue"></param>
        /// <param name="outSession"></param>
        public void Deconstruct(out T outValue, out Session outSession)
        {
            outValue = value;
            outSession = session;
        }
    }

    /// <summary>
    /// Stub implementation for session, can be used in tests to simplify code.
    /// </summary>
    public class StubSession : Session
    {
        /// <inheritdoc />
        public StubSession()
        {
        }

        /// <summary>
        /// Sets <see cref="Session.Current"/> session to use <see cref="Session.StubSession"/>
        /// </summary>
        public static void UseThis()
        {
            Current = new StubSession();
        }
    }
}