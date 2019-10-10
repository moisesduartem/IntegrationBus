using System;
using System.IO;
using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace IntegrationBusRabbitMq
{
    public interface IBusConnectionPersister : IDisposable
    {
        bool IsConnected { get; }

        IModel CreateModel();
        bool TryConnect();
    }

    public class BusConnectionPersister : IBusConnectionPersister
    {
        /// <summary>
        /// RabbitMq connection factory.
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;
     
        private readonly int _retryAttempts;

        /// <summary>
        /// AMQP connection.
        /// </summary>
        private IConnection _connection;
        private bool _disposed;

        private readonly object _connectionLock = new object();

        public BusConnectionPersister(IConnectionFactory connectionFactory, int retryAttempts)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryAttempts = retryAttempts;
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        /// <summary>
        /// Create and return a new session and model to connection.
        /// </summary>
        /// <returns></returns>
        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Nenhuma conexão com RabbitMq está disponível para completar esta ação.");

            return _connection.CreateModel();
        }

        /// <summary>
        /// Execute connection try to RabbitMq.
        /// </summary>
        /// <returns></returns>
        public bool TryConnect()
        {
            lock (_connectionLock)
            {
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

        public void Dispose()
        {
            if (!_disposed) return;

            _disposed = true;
            _connection.Dispose();
        }
    }
}
