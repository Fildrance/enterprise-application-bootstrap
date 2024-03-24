using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.SystemEvents;

/// <summary>
/// Manager for system events (<seealso cref="SystemEventName"/>) of application.
/// </summary>
[PublicAPI]
public abstract class SystemEventsManager
{
    /// <summary>
    /// Gets awaitable object for system event of certain name. It can be any event, but events that already happened will return completed task.
    /// </summary>
    /// <param name="eventName">Name of event.</param>
    /// <returns>Task, that will is completed or it will be complete after requested event.</returns>
    /// <exception cref="ArgumentNullException">in case <see cref="eventName"/>=null.</exception>
    [NotNull]
    public abstract Task Get([NotNull] SystemEventName eventName);

    /// <summary>
    /// Marks event as complete.
    /// </summary>
    /// <param name="eventName">Name of event to mark.</param>
    /// <exception cref="ArgumentNullException">in case <see cref="eventName"/>=null.</exception>
    public abstract void Complete([NotNull] SystemEventName eventName);

    /// <summary>
    /// Default <see cref="SystemEventsManager"/> implementation.
    /// </summary>
    [PublicAPI]
    public class DefaultSystemEventsManager : SystemEventsManager
    {
        /// <summary>
        /// This single container should be used by any derived implementation to prevent losing links to tasks during runtime.
        /// </summary>
        protected readonly IDictionary<SystemEventName, AwaitableEvent> EventsContainer = new ConcurrentDictionary<SystemEventName, AwaitableEvent>();

        /// <inheritdoc />
        public override Task Get(SystemEventName eventName)
        {
            var awaitableEvent = GetOrCreate(eventName);

            return awaitableEvent.Awaitable;
        }

        /// <inheritdoc />
        public override void Complete(SystemEventName eventName)
        {
            var awaitableEvent = GetOrCreate(eventName);
            awaitableEvent.Complete();
        }

        /// <summary>
        /// Get awaitable event or creates new one if none were existing.
        /// </summary>
        /// <param name="eventName">Name of system event to be awaited.</param>
        /// <returns>Awaitable for requested event.</returns>
        [NotNull]
        protected AwaitableEvent GetOrCreate([NotNull] SystemEventName eventName)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            if (!EventsContainer.TryGetValue(eventName, out var awaitableEvent))
            {
                awaitableEvent = new AwaitableEvent();
                EventsContainer.Add(eventName, awaitableEvent);
            }

            return awaitableEvent;
        }
    }
}