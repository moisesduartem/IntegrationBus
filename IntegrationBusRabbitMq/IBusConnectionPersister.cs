using System;
using RabbitMQ.Client;

namespace IntegrationBusRabbitMq
{
    public interface IBusConnectionPersister : IDisposable
    {
        bool IsConnected { get; }

        IModel TakeAChannel();
        void GiveChannel(IModel channel);
        void SaveConsumerChannel(IModel channel);

        bool TryConnect();
    }
}