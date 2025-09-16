using System;
using System.Threading;
using System.Threading.Tasks;
using Fin_Backend.Infrastructure.Messaging;
using Fin_Backend.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.RabbitMq;
using Xunit;

namespace Fin_Backend.Tests.Integration
{
    public class MessageBrokerTests : IAsyncLifetime
    {
        private readonly RabbitMqContainer _rabbitMqContainer;
        private IConnection _connection;
        private IModel _channel;
        private MessageBroker _messageBroker;

        public MessageBrokerTests()
        {
            _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.11-management-alpine")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start RabbitMQ container
            await _rabbitMqContainer.StartAsync();

            // Create connection factory
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqContainer.Hostname,
                Port = _rabbitMqContainer.GetMappedPort(5672),
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            // Create connection and channel
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Create and configure message broker
            var rabbitMqSettings = new RabbitMQSettings
            {
                Host = _rabbitMqContainer.Hostname,
                Port = _rabbitMqContainer.GetMappedPort(5672),
                Username = "guest",
                Password = "guest",
                VirtualHost = "/"
            };

            var eventBusSettings = new EventBusSettings
            {
                SubscriptionClientName = "test-client",
                RetryCount = 3
            };

            var options = Options.Create(rabbitMqSettings);
            var eventBusOptions = Options.Create(eventBusSettings);

            _messageBroker = new MessageBroker(options, eventBusOptions);
        }

        public async Task DisposeAsync()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            await _rabbitMqContainer.DisposeAsync();
        }

        [Fact]
        public async Task PublishMessage_ValidMessage_MessageReceivedByConsumer()
        {
            // Arrange
            var testQueueName = "test-queue";
            var testExchangeName = "test-exchange";
            var testRoutingKey = "test-routing-key";
            var testMessage = new { Id = 1, Name = "Test Message", Timestamp = DateTime.UtcNow };
            var messageReceived = false;
            string receivedMessage = null;
            var waitHandle = new ManualResetEvent(false);

            // Declare exchange and queue
            _channel.ExchangeDeclare(testExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(testQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(testQueueName, testExchangeName, testRoutingKey);

            // Setup consumer
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                receivedMessage = System.Text.Encoding.UTF8.GetString(body);
                messageReceived = true;
                waitHandle.Set();
            };

            _channel.BasicConsume(testQueueName, autoAck: true, consumer: consumer);

            // Act
            await _messageBroker.PublishAsync(testExchangeName, testRoutingKey, testMessage);

            // Assert
            var signaled = waitHandle.WaitOne(TimeSpan.FromSeconds(5));
            signaled.Should().BeTrue("Message should be received within timeout");
            messageReceived.Should().BeTrue("Message should be received by consumer");
            receivedMessage.Should().NotBeNullOrEmpty("Received message should not be null or empty");
            receivedMessage.Should().Contain(testMessage.Name, "Received message should contain the original message content");
        }

        [Fact]
        public async Task SubscribeToQueue_PublishMessage_CallbackInvoked()
        {
            // Arrange
            var testQueueName = "test-callback-queue";
            var testExchangeName = "test-callback-exchange";
            var testRoutingKey = "test-callback-routing-key";
            var testMessage = new TestMessage { Id = 2, Name = "Callback Test", Timestamp = DateTime.UtcNow };
            var callbackInvoked = false;
            TestMessage receivedMessage = null;
            var waitHandle = new ManualResetEvent(false);

            // Declare exchange and queue
            _channel.ExchangeDeclare(testExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(testQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(testQueueName, testExchangeName, testRoutingKey);

            // Setup subscription callback
            await _messageBroker.SubscribeAsync<TestMessage>(testQueueName, message =>
            {
                receivedMessage = message;
                callbackInvoked = true;
                waitHandle.Set();
                return Task.CompletedTask;
            });

            // Act
            await _messageBroker.PublishAsync(testExchangeName, testRoutingKey, testMessage);

            // Assert
            var signaled = waitHandle.WaitOne(TimeSpan.FromSeconds(5));
            signaled.Should().BeTrue("Callback should be invoked within timeout");
            callbackInvoked.Should().BeTrue("Callback should be invoked");
            receivedMessage.Should().NotBeNull("Received message should not be null");
            receivedMessage.Id.Should().Be(testMessage.Id, "Received message ID should match original message");
            receivedMessage.Name.Should().Be(testMessage.Name, "Received message name should match original message");
        }

        [Fact]
        public async Task PublishToNonExistentExchange_ThrowsException()
        {
            // Arrange
            var nonExistentExchange = "non-existent-exchange";
            var routingKey = "test-routing-key";
            var message = new { Id = 3, Name = "Error Test" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _messageBroker.PublishAsync(nonExistentExchange, routingKey, message));
        }

        public class TestMessage
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}