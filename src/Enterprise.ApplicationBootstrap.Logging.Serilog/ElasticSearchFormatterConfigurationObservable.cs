using System;
using System.Collections.Generic;
using Enterprise.ApplicationBootstrap.Logging.Serilog.Settings;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Enterprise.ApplicationBootstrap.Logging.Serilog;

/// <summary>
/// Observable for tracking <see cref="CustomElasticLogFormatterSettings"/>
/// and sending notifications of change into <see cref="InvalidFieldHandlingElasticSearchJsonFormatter"/>.
/// </summary>
public class ElasticSearchFormatterConfigurationObservable : IObservable<CustomElasticLogFormatterSettings>, IDisposable
{
    // ReSharper disable once InconsistentNaming
    private static readonly ElasticSearchFormatterConfigurationObservable _instance = new();

    /// <summary>
    /// Public access for observable object.
    /// </summary>
    [NotNull]
    public static IObservable<CustomElasticLogFormatterSettings> Instance => _instance;

    /// <summary>
    /// Sets monitor options.
    /// </summary>
    /// <param name="options">Link to monitor options to set in <see cref="Instance"/> serilog-formatter.</param>
    public static void Init([NotNull] IOptionsMonitor<CustomElasticLogFormatterSettings> options)
    {
        _instance.SetOptions(options);
    }

    /// <summary>
    /// Container for action to be executed on dispose.
    /// </summary>
    private class Unsubscriber(Action unsubscribeAction) : IDisposable
    {
        private bool _isCalled;

        public void Dispose()
        {
            if (!_isCalled)
            {
                unsubscribeAction();
                _isCalled = true;
            }
        }
    }

    private readonly List<IObserver<CustomElasticLogFormatterSettings>> _observers = new();
    private IDisposable _optionsSubscription;
    private CustomElasticLogFormatterSettings _currentValue;

    private ElasticSearchFormatterConfigurationObservable()
    {
    }

    /// <summary>
    /// Subscribes observer to settings changes.
    /// </summary>
    /// <returns>Dispose that lets unsubscribe <see cref="observer"/> from change notifications.</returns>
    public IDisposable Subscribe(IObserver<CustomElasticLogFormatterSettings> observer)
    {
        _observers.Add(observer);
        observer.OnNext(_currentValue!);
        return new Unsubscriber(() => _observers.Remove(observer));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _optionsSubscription?.Dispose();
    }

    private void SetOptions(IOptionsMonitor<CustomElasticLogFormatterSettings> options)
    {
        DoNotifyAll(_currentValue);

        _optionsSubscription?.Dispose();
        _optionsSubscription = options.OnChange(DoNotifyAll);
    }

    private void DoNotifyAll(CustomElasticLogFormatterSettings x)
    {
        _currentValue = x;
        foreach (var obs in _observers)
        {
            obs.OnNext(x!);
        }
    }
}