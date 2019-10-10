using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IntegrationBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace IntegrationBusRabbitMq
{
    public class IntegrationBusRabbitMq : IIntegrationBus, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly IBusConnectionPersister _connectionPersister;
        private readonly IMemorySubscriptionManager _inMemorySubscriptionManager;

        private readonly string _brokerName;
        private readonly string _queueName;

        private const string BROKER_TYPE = "topic";

        private readonly int _publishRetryAttempts;
        private readonly int _consumeRetryAttempts;

        private IModel _consumerChannel;

        public IntegrationBusRabbitMq(
            IServiceProvider serviceProvider,
            (string brokerName, string queueName) configurationNames,
            (int publishRetryAttempts, int consumeRetryAttempts) retryAttempts)
        {
            _connectionPersister = serviceProvider.GetRequiredService<IBusConnectionPersister>() 
                                   ?? throw new ArgumentNullException(nameof(_connectionPersister));

            _inMemorySubscriptionManager = serviceProvider.GetRequiredService<IMemorySubscriptionManager>() 
                                           ?? new MemorySubscriptionManager();

            _serviceProvider = serviceProvider;

            _brokerName = configurationNames.brokerName;
            _queueName = configurationNames.queueName;

            _publishRetryAttempts = retryAttempts.publishRetryAttempts;
            _consumeRetryAttempts = retryAttempts.consumeRetryAttempts;

            _consumerChannel = CreateConsumerChannel(_queueName);
        }

        private IModel CreateConsumerChannel(string queueName)
        {
            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            var channel = _connectionPersister.CreateModel();

            channel.ExchangeDeclare(_brokerName, BROKER_TYPE);
            channel.QueueDeclare(queueName, true, false, false, null);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();

                _consumerChannel = CreateConsumerChannel(queueName);
                StartBasicConsume();
            };

            return channel;
        }

        private void StartBasicConsume()
        {
            if (_consumerChannel == null) return;

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += ConsumerReceived;

            _consumerChannel.BasicConsume(_queueName, false, consumer);
        }

        private async Task ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body);

            if (_inMemorySubscriptionManager.HasHandlerForEvent(eventName))
            {
                var handlers = _inMemorySubscriptionManager.GetHandlersForEvent(eventName);
                foreach (var handler in handlers)
                {
                    var policyContext = new Context(eventName);

                    var policyBuilder = Policy
                        .HandleResult<bool>(successfullyExecution => !successfullyExecution)
                        .Or<Exception>();

                    var policy = policyBuilder
                        .WaitAndRetryAsync(_consumeRetryAttempts, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                        .WithPolicyKey(eventName);

                    var policyExecution = await policy.ExecuteAsync(
                        async ctx => await ProcessEvent(eventName, message, handler).ConfigureAwait(false), policyContext).ConfigureAwait(false);

                    if (!policyExecution)
                    {
                        // todo: put some dead queue like 'error-queue' redirect here
                    }

                    _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
                }
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message, Type handlerType)
        {
            var integrationEventType = _inMemorySubscriptionManager.GetEventType(eventName);
            var integrationEvent = JsonConvert.DeserializeObject(message, integrationEventType);

            using (var scope = _serviceProvider.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType);

                var result = (Task<bool>) concreteType
                    .GetMethod("Handle")
                    .Invoke(handler, new[] { integrationEvent });

                return await result;
            }

        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            var policyBuilder = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>();

            var policy = policyBuilder.WaitAndRetry(
                _publishRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            CreateModelAndPublish(@event, policy);
        }

        private void CreateModelAndPublish(IntegrationEvent @event, RetryPolicy policy)
        {
            using (var channel = _connectionPersister.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.ExchangeDeclare(_brokerName, BROKER_TYPE);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    channel.BasicPublish(_brokerName, eventName, true, properties, body);
                });
            }
        }

        public void Subscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent where TEventHandler : IIntegrationEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            DoInternalSubscription(eventName);

            _inMemorySubscriptionManager.AddEventSubscription<TEvent, TEventHandler>();
            StartBasicConsume();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _inMemorySubscriptionManager.HasHandlerForEvent(eventName);
            if (containsKey) return;

            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            using (var channel = _connectionPersister.CreateModel())
                channel.QueueBind(_queueName, _brokerName, eventName);
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();
            _inMemorySubscriptionManager.Clear();
        }
    }
}