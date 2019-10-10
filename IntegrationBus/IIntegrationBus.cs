using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationBus
{
    public interface IIntegrationBus
    {
        /// <summary>
        /// Publish a event to service queue with de routing key the same as @event name.
        /// </summary>
        /// <param name="event"></param>
        void Publish(IntegrationEvent @event);

        /// <summary>
        /// Subscribe a event with event handler.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent 
            where TEventHandler : IIntegrationEventHandler<TEvent>;
    }
}
