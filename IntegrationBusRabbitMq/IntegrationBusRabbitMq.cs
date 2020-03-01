using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IntegrationBus;
using IntegrationBusRabbitMq.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
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

        private readonly string _gatewayBrokerName;
        private readonly string _serviceName;

        private const string GatewayBrokerType = "topic";
        private const string ServiceBrokerType = "fanout";

        private const string PublishRetryAttemptsConfigKey = "PublishRetryAttempts";
        private const string ConsumeRetryAttemptsConfigKey = "ConsumeRetryAttempts";
        private const string ConsumeConcurrentsDefaultAmountConfigKey = "ConsumeConcurrentsDefaultAmount";

        private readonly IDictionary<string, object> _configuration;

        public static object AckLock = new object();
        private bool _enableConsume = false;

        public IntegrationBusRabbitMq(
            IServiceProvider serviceProvider,
            (string brokerName, string serviceName) brokers,
            (int publishRetryAttempts, int consumeRetryAttempts) retryAttempts,
            int consumeConcurrentsDefaultAmount)
        {
            _connectionPersister = serviceProvider.GetRequiredService<IBusConnectionPersister>() 
                                   ?? throw new ArgumentNullException(nameof(_connectionPersister));

            _inMemorySubscriptionManager = serviceProvider.GetRequiredService<IMemorySubscriptionManager>() 
                                           ?? new MemorySubscriptionManager();

            _serviceProvider = serviceProvider;

            _gatewayBrokerName = brokers.brokerName;
            _serviceName = brokers.serviceName;

            _configuration = new Dictionary<string, object>
            {
                [PublishRetryAttemptsConfigKey] = retryAttempts.publishRetryAttempts,
                [ConsumeRetryAttemptsConfigKey] = retryAttempts.consumeRetryAttempts,
                [ConsumeConcurrentsDefaultAmountConfigKey] = consumeConcurrentsDefaultAmount
            };
        }

        private IModel CreateConsumerChannel()
        {
            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            var channel = _connectionPersister.TakeAChannel();
            

            //channel.CallbackException += (sender, ea) =>
            //{
            //    _consumerChannel.Dispose();

            //    _consumerChannel = CreateConsumerChannel(queueName);
            //    StartBasicConsume();
            //};

            return channel;
        }

        private void StartBasicConsume(IModel channel, string eventName, string consumerTag)
        {  
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += ConsumerReceived;

            channel.BasicConsume(eventName, false, consumer);
            _connectionPersister.SaveConsumerChannel(channel);
        }

        private async Task ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            while (!_enableConsume) { }

            var routingKey = eventArgs.RoutingKey;

            var message = Encoding.UTF8.GetString(eventArgs.Body);

            if (_inMemorySubscriptionManager.HasHandlerForEvent(routingKey))
            {
                var handlers = _inMemorySubscriptionManager.GetHandlersForEvent(routingKey);
                var handlersCount = handlers.Count();

                var handleTasks = new Task<bool>[handlersCount];

                var k = 0;
                foreach (var handler in handlers)
                {
                    var handleTask = Task.Run(async () =>
                    {
                        var consumerRetryAttemptsConfigKey = (int)_configuration[ConsumeRetryAttemptsConfigKey];

                        var policyContext = new Context(routingKey);

                        var policyBuilder = Policy
                            .HandleResult<bool>(successfullyExecution => !successfullyExecution)
                            .Or<Exception>();

                        var policy = policyBuilder
                            .WaitAndRetryAsync(consumerRetryAttemptsConfigKey, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                            .WithPolicyKey(routingKey);

                        var policyExecution = await policy.ExecuteAndCaptureAsync(
                            async ctx => await ProcessEvent(routingKey, message, handler).ConfigureAwait(false), policyContext).ConfigureAwait(false);

                        return policyExecution.Outcome == OutcomeType.Successful;
                    });

                    handleTasks[k] = handleTask;
                    k++;
                }

                try
                {
                    await Task.WhenAll(handleTasks);
                }
                catch (Exception ex)
                {
                    // todo: put some dead queue redirect here like 'error-queue' and remove throw
                    throw ex;
                }
                finally
                {
                    var channel = ((AsyncEventingBasicConsumer) sender).Model;
                    channel.BasicAck(eventArgs.DeliveryTag, false);
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
                var concreteType = typeof(IIntegrationEventHandler2<>).MakeGenericType(integrationEventType);

                var result = (Task<bool>) handlerType
                    .GetMethod("Handle")
                    .Invoke(handler, new[] { integrationEvent });

                return await result;
            }

        }

        public bool Publish(IntegrationEvent @event)
        {
            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            var publishRetryAttempts = (int) _configuration[PublishRetryAttemptsConfigKey];

            var policyBuilder = Policy
                    .Handle<BrokerUnreachableException>()
                    .Or<SocketException>();

            var policy = policyBuilder.WaitAndRetry(
                publishRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var policyResult = policy.ExecuteAndCapture(() => CreateModelAndPublish(@event));

            return policyResult.Outcome == OutcomeType.Successful;
        }

        private void CreateModelAndPublish(IntegrationEvent @event)
        {
            var channel = _connectionPersister.TakeAChannel();
            
            var eventName = @event.GetType().Name;

            channel.ExchangeDeclare(_gatewayBrokerName, GatewayBrokerType);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            channel.BasicPublish(_gatewayBrokerName, eventName, true, properties, body);

            _connectionPersister.GiveChannel(channel);            
        }

        public ISubscription Subscribe<TEvent, TEventHandler>(int consumersAmount = -1) 
            where TEvent : IntegrationEvent
            where TEventHandler : IIntegrationEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var subscriptionName = typeof(TEventHandler).Name;

            var consumeConcurrentsDefaultAmount = (int) _configuration[ConsumeConcurrentsDefaultAmountConfigKey];
            if (consumersAmount <= 0) consumersAmount = consumeConcurrentsDefaultAmount;
            
            DeclareQueueAndOpenConsumers(queueNameSuffix: subscriptionName, routingKey: eventName, consumersAmount);

            var subscription 
                = _inMemorySubscriptionManager.AddAndRetrieveEventSubscription<TEvent, TEventHandler>();

            return subscription;
        }

        private void DeclareQueueAndOpenConsumers(string queueNameSuffix, string routingKey, int consumersAmount)
        {
            //var containsKey = _inMemorySubscriptionManager.HasHandlerForEvent(queueAndRoutingKey);
            //if (containsKey) return;

            if (!_connectionPersister.IsConnected) _connectionPersister.TryConnect();

            var channel = _connectionPersister.TakeAChannel();

            var queueName = $"{routingKey}#{queueNameSuffix}";
            var eventBrokerName = $"{_serviceName}#{routingKey}";

            channel.ExchangeDeclare(_gatewayBrokerName, GatewayBrokerType);

            channel.ExchangeDeclare(eventBrokerName, ServiceBrokerType);
            channel.ExchangeBind(eventBrokerName, _gatewayBrokerName, routingKey);

            channel.QueueDeclare(queueName, true, false, false, null);
            channel.QueueBind(queueName, eventBrokerName, "");

            for (var c = 1; c <= consumersAmount; c++)
            {
                var consumeChannel = _connectionPersister.TakeAChannel();
                StartBasicConsume(consumeChannel, queueName, $"{queueName}#{c}");
            }           
        }

        public void EnableConsume() => _enableConsume = true;

        public void Dispose()
        {
            _inMemorySubscriptionManager.Clear();
        }
    }
}