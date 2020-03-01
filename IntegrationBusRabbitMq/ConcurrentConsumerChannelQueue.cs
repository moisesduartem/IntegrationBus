using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace IntegrationBusRabbitMq
{
    public class ConcurrentConsumerChannelQueue : ConcurrentQueue<IModel>
    {
    }
}
