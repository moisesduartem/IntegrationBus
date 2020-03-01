using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using IntegrationBusRabbitMq.Exceptions;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace IntegrationBusRabbitMq
{
    public class BusConnectionPersister : IBusConnectionPersister
    {
        // RabbitMq connection factory
        private readonly IConnectionFactory _connectionFactory;
     
        private readonly int _retryAttempts;

        // AMQP connection
        private IConnection _connection;
        private bool _disposed;

        private readonly object _connectionLock = new object();

        private readonly ConcurrentBag<IModel> _channelPool;
        private readonly ConcurrentBag<IModel> _consumerPool;

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;
               
        public BusConnectionPersister(IConnectionFactory connectionFactory, int retryAttempts)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryAttempts = retryAttempts;

            _channelPool = new ConcurrentBag<IModel>();
            _consumerPool = new ConcurrentBag<IModel>();
        }
       
        public bool TryConnect()
        {
            lock (_connectionLock)
            {
                if (IsConnected) return true;

                var policy = Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryAttempts, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                policy.Execute(() => _connection = _connectionFactory.CreateConnection());

                if (!IsConnected) return false;

                HandleConnectionPersisted();
                return true;
            }
        }
       
        private void HandleConnectionPersisted()
        {
            _connection.ConnectionBlocked += OnConnectionBlocked;
            _connection.CallbackException += OnCallbackException;
            _connection.ConnectionShutdown += OnConnectionShutdown;
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            TryConnect();
        }

        public IModel TakeAChannel()
        {
            if (!IsConnected) throw new MissConnectionException();
            if (_channelPool == null) throw new Exception("Não pode estar nulo!");

            var taked = _channelPool.TryTake(out var channel);
            if (taked) return channel;

            return _connection.CreateModel();
        }

        public void GiveChannel(IModel channel) => _channelPool.Add(channel);

        public void SaveConsumerChannel(IModel channel) => _consumerPool.Add(channel);

        public void Dispose()
        {
            if (!_disposed) return;

            _disposed = true;

            _channelPool.Clear();
            _consumerPool.Clear();

            _connection.Dispose();
        }
    }
}
