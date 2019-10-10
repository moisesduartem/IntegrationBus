using System;
using System.Collections.Generic;

namespace IntegrationBus
{
    public interface IMemorySubscriptionManager
    {
        /// <summary>
        /// Add a subscription, the event name is the type of event created child of IntegrationService. 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        void AddEventSubscription<TEvent, TEventHandler>() 
            where TEvent : IntegrationEvent
            where TEventHandler : IIntegrationEventHandler<TEvent>;

        /// <summary>
        /// Get all handlers for giving event name.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        IEnumerable<Type> GetHandlersForEvent(string eventName);
      
        Type GetEventType(string eventName);

        /// <summary>
        /// Check if exist some handler to giving type.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        bool HasHandlerForEvent(string eventName);

        void Clear();
    }
}