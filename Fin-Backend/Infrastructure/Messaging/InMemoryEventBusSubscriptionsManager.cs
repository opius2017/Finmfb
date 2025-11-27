using System;
using System.Collections.Generic;
using System.Linq;

namespace FinTech.Infrastructure.Messaging
{
    /// <summary>
    /// In-memory implementation of the event bus subscriptions manager
    /// </summary>
    public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        /// <summary>
        /// Event raised when an event is removed
        /// </summary>
        public event EventHandler<string> OnEventRemoved;

        /// <summary>
        /// Creates a new in-memory event bus subscriptions manager
        /// </summary>
        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }

        /// <summary>
        /// Checks if the event bus is empty
        /// </summary>
        public bool IsEmpty => !_handlers.Keys.Any();

        /// <summary>
        /// Clears all subscriptions
        /// </summary>
        public void Clear() => _handlers.Clear();

        /// <summary>
        /// Adds a subscription
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        public void AddSubscription<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscription(typeof(TH), eventName);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        public void RemoveSubscription<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();

            DoRemoveHandler(eventName, handlerToRemove);
        }

        /// <summary>
        /// Gets the event key from the event type
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>The event key</returns>
        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }

        /// <summary>
        /// Gets all subscriptions for an event
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The list of subscriptions</returns>
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        /// <summary>
        /// Gets all subscriptions for an event
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>The list of subscriptions</returns>
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IIntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        /// <summary>
        /// Checks if a subscription exists
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>True if the subscription exists, otherwise false</returns>
        public bool HasSubscriptionsForEvent<T>() where T : IIntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        /// <summary>
        /// Checks if a subscription exists
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>True if the subscription exists, otherwise false</returns>
        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        /// <summary>
        /// Gets the event type by name
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The event type</returns>
        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

        private void DoAddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(new SubscriptionInfo(handlerType));
        }

        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }
    }
}
