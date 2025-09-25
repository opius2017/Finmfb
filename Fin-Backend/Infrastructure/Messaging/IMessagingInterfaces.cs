using System;
using System.Threading.Tasks;

namespace Fin_Backend.Infrastructure.Messaging
{
    /// <summary>
    /// Interface for integration events
    /// </summary>
    public interface IIntegrationEvent
    {
        /// <summary>
        /// Gets the unique identifier for this integration event
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Gets the creation date for this integration event
        /// </summary>
        DateTime CreationDate { get; }
    }
    
    /// <summary>
    /// Base class for all integration events
    /// </summary>
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        /// <summary>
        /// Gets the unique identifier for this integration event
        /// </summary>
        public Guid Id { get; private set; }
        
        /// <summary>
        /// Gets the creation date for this integration event
        /// </summary>
        public DateTime CreationDate { get; private set; }
        
        /// <summary>
        /// Creates a new integration event
        /// </summary>
        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Creates a new integration event with specified ID and creation date
        /// </summary>
        /// <param name="id">The event ID</param>
        /// <param name="creationDate">The event creation date</param>
        protected IntegrationEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }
    }
    
    /// <summary>
    /// Interface for integration event handlers
    /// </summary>
    /// <typeparam name="TIntegrationEvent">The type of integration event</typeparam>
    public interface IIntegrationEventHandler<in TIntegrationEvent> 
        where TIntegrationEvent : IIntegrationEvent
    {
        /// <summary>
        /// Handles the integration event
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task HandleAsync(TIntegrationEvent @event);
    }
    
    /// <summary>
    /// Interface for the event bus
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an integration event to the event bus
        /// </summary>
        /// <param name="event">The integration event to publish</param>
        Task PublishAsync(IIntegrationEvent @event);
        
        /// <summary>
        /// Subscribes to an integration event
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        void Subscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>;
            
        /// <summary>
        /// Unsubscribes from an integration event
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        void Unsubscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
    
    /// <summary>
    /// Interface for managing event bus subscriptions
    /// </summary>
    public interface IEventBusSubscriptionsManager
    {
        /// <summary>
        /// Checks if the event bus is empty
        /// </summary>
        bool IsEmpty { get; }
        
        /// <summary>
        /// Event raised when an event is removed
        /// </summary>
        event EventHandler<string> OnEventRemoved;
        
        /// <summary>
        /// Adds a subscription
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        void AddSubscription<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>;
            
        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <typeparam name="TH">The type of integration event handler</typeparam>
        void RemoveSubscription<T, TH>()
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>;
            
        /// <summary>
        /// Checks if a subscription exists
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>True if the subscription exists, otherwise false</returns>
        bool HasSubscriptionsForEvent<T>() where T : IIntegrationEvent;
        
        /// <summary>
        /// Checks if a subscription exists
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>True if the subscription exists, otherwise false</returns>
        bool HasSubscriptionsForEvent(string eventName);
        
        /// <summary>
        /// Gets the event type by name
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The event type</returns>
        Type GetEventTypeByName(string eventName);
        
        /// <summary>
        /// Clears all subscriptions
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Gets all subscriptions for an event
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        /// <returns>The list of subscriptions</returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        
        /// <summary>
        /// Gets all subscriptions for an event
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>The list of subscriptions</returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IIntegrationEvent;
        
        /// <summary>
        /// Gets the event name from the event type
        /// </summary>
        /// <typeparam name="T">The type of integration event</typeparam>
        /// <returns>The name of the event</returns>
        string GetEventKey<T>();
    }
    
    /// <summary>
    /// Subscription information
    /// </summary>
    public class SubscriptionInfo
    {
        /// <summary>
        /// Gets the event handler type
        /// </summary>
        public Type HandlerType { get; }
        
        /// <summary>
        /// Creates a new subscription info
        /// </summary>
        /// <param name="handlerType">The event handler type</param>
        public SubscriptionInfo(Type handlerType)
        {
            HandlerType = handlerType;
        }
    }
    
    /// <summary>
    /// Interface for the integration event outbox service
    /// </summary>
    public interface IIntegrationEventOutboxService
    {
        /// <summary>
        /// Saves an integration event to the outbox
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SaveEventAsync(IIntegrationEvent @event);
        
        /// <summary>
        /// Publishes all pending events from the outbox
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PublishEventsAsync();
    }
    
    /// <summary>
    /// Interface for the persistent RabbitMQ connection
    /// </summary>
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the connection is connected
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Creates a model (channel)
        /// </summary>
        /// <returns>The created model</returns>
        IModel CreateModel();
        
        /// <summary>
        /// Tries to connect to RabbitMQ
        /// </summary>
        /// <returns>True if connected successfully, otherwise false</returns>
        bool TryConnect();
    }
}
