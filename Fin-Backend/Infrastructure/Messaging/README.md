# Message Broker Integration

This document provides an overview of the message broker integration implemented in the Finmfb banking application.

## Architecture

The message broker integration follows an event-driven architecture using the following components:

1. **RabbitMQ**: Used as the message broker to handle asynchronous communication between services
2. **Integration Events**: Domain events that are published to the message broker
3. **Event Handlers**: Services that subscribe to and process integration events
4. **Outbox Pattern**: Ensures reliable message delivery by storing events in a database before publishing

## Components

### Integration Events

Integration events represent important business events that need to be communicated across different services. Examples include:

- `PaymentProcessedIntegrationEvent`: Published when a payment is processed
- `AccountCreatedIntegrationEvent`: Published when a new account is created
- `LoanApprovedIntegrationEvent`: Published when a loan is approved

### Event Bus

The event bus is responsible for publishing and subscribing to integration events:

- `IEventBus`: Interface for the event bus
- `RabbitMQEventBus`: RabbitMQ implementation of the event bus
- `IEventBusSubscriptionsManager`: Manages event subscriptions

### Outbox Pattern

The outbox pattern ensures reliable message delivery by:

1. Storing integration events in a database table (`IntegrationEventOutbox`)
2. Using a background service (`OutboxProcessorService`) to publish events from the outbox
3. Marking events as published after successful delivery

## Usage

### Publishing Events

To publish an integration event:

```csharp
// Create the integration event
var paymentProcessedEvent = new PaymentProcessedIntegrationEvent(
    paymentId,
    accountId,
    amount,
    currency,
    paymentMethod,
    status,
    transactionReference,
    paymentDate);

// Save the event to the outbox
await _outboxService.SaveEventAsync(paymentProcessedEvent);
```

### Subscribing to Events

To subscribe to an integration event:

```csharp
// Register the event handler in DI
services.AddTransient<PaymentProcessedIntegrationEventHandler>();

// Subscribe to the event
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
eventBus.Subscribe<PaymentProcessedIntegrationEvent, PaymentProcessedIntegrationEventHandler>();
```

### Handling Events

To handle an integration event:

```csharp
public class PaymentProcessedIntegrationEventHandler : IIntegrationEventHandler<PaymentProcessedIntegrationEvent>
{
    public async Task HandleAsync(PaymentProcessedIntegrationEvent @event)
    {
        // Process the event
        // ...
    }
}
```

## Configuration

RabbitMQ and event bus settings are configured in `appsettings.json`:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  },
  "EventBus": {
    "SubscriptionClientName": "fintech",
    "RetryCount": 5
  }
}
```

## Benefits

1. **Decoupling**: Services can communicate without direct dependencies
2. **Scalability**: Services can scale independently
3. **Reliability**: The outbox pattern ensures messages are not lost
4. **Resilience**: Services can continue to operate even if other services are down
5. **Extensibility**: New consumers can be added without modifying existing code