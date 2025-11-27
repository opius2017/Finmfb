using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

namespace FinTech.Infrastructure.Messaging
{
    /// <summary>
    /// Service registration for message broker components
    /// </summary>
    public static class MessageBrokerRegistration
    {
        /// <summary>
        /// Add message broker services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure and register RabbitMQ connection factory
            var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>() 
                ?? throw new InvalidOperationException("RabbitMQ settings are missing from configuration");
            
            services.AddSingleton(rabbitMqSettings);
            
            services.AddSingleton<IConnectionFactory>(sp => 
            {
                var settings = sp.GetRequiredService<RabbitMQSettings>();
                return new ConnectionFactory
                {
                    HostName = settings.Host,
                    Port = settings.Port,
                    UserName = settings.Username,
                    Password = settings.Password,
                    VirtualHost = settings.VirtualHost,
                    DispatchConsumersAsync = true
                };
            });
            
            services.AddSingleton<IRabbitMQPersistentConnection, RabbitMQPersistentConnection>();
            
            // Register event bus
            services.AddSingleton<IEventBus, RabbitMQEventBus>(sp => 
            {
                var persistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
                var eventBusSettings = configuration.GetSection("EventBus").Get<EventBusSettings>() 
                    ?? throw new InvalidOperationException("EventBus settings are missing from configuration");
                var serviceProvider = sp;
                
                return new RabbitMQEventBus(
                    persistentConnection,
                    logger,
                    serviceProvider,
                    eventBusSettings.SubscriptionClientName,
                    eventBusSettings.RetryCount);
            });
            
            // Register outbox service
            services.AddScoped<IIntegrationEventOutboxService, IntegrationEventOutboxService>();
            
            // Register event handlers
            RegisterEventHandlers(services);
            
            return services;
        }
        
        private static void RegisterEventHandlers(IServiceCollection services)
        {
            // Register all event handlers
            services.AddTransient<PaymentProcessedIntegrationEventHandler>();
            services.AddTransient<AccountCreatedIntegrationEventHandler>();
            services.AddTransient<LoanApprovedIntegrationEventHandler>();
            
            // Register subscriptions
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }
    }
    
    /// <summary>
    /// RabbitMQ connection settings
    /// </summary>
    public class RabbitMQSettings
    {
        /// <summary>
        /// RabbitMQ host
        /// </summary>
        public string Host { get; set; } = "localhost";
        
        /// <summary>
        /// RabbitMQ port
        /// </summary>
        public int Port { get; set; } = 5672;
        
        /// <summary>
        /// RabbitMQ username
        /// </summary>
        public string Username { get; set; } = "guest";
        
        /// <summary>
        /// RabbitMQ password
        /// </summary>
        public string Password { get; set; } = "guest";
        
        /// <summary>
        /// RabbitMQ virtual host
        /// </summary>
        public string VirtualHost { get; set; } = "/";
    }
    
    /// <summary>
    /// Event bus settings
    /// </summary>
    public class EventBusSettings
    {
        /// <summary>
        /// Name of the subscription client
        /// </summary>
        public string SubscriptionClientName { get; set; } = "fintech";
        
        /// <summary>
        /// Number of retry attempts for publishing events
        /// </summary>
        public int RetryCount { get; set; } = 5;
    }
}
