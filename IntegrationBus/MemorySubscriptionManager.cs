using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using IntegrationBus.Exceptions;

namespace IntegrationBus
{
    public class MemorySubscriptionManager : IMemorySubscriptionManager
    {
        private readonly IDictionary<string, Subscription> _subscriptions;

        public MemorySubscriptionManager()
        {
            _subscriptions = new Dictionary<string, Subscription>();
        }
       
        public ISubscription AddAndRetrieveEventSubscription<TEvent, TEventHandler>() 
            where TEvent : IntegrationEvent 
            where TEventHandler : IIntegrationEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var eventHandlerType = typeof(TEventHandler);

            if (!HasHandlerForEvent(eventType.Name))
            {
                var newSubscription = new Subscription(eventType);
                newSubscription.AddHandler(eventHandlerType);

                _subscriptions.Add(eventType.Name, newSubscription);

                return newSubscription;
            }

            if (_subscriptions[eventType.Name].EventHandlerTypes.Any(h => h == eventHandlerType))
                throw new SubscriptionDuplicateException(eventHandlerType, eventType.Name);

            var subscription = _subscriptions[eventType.Name];
            subscription.AddHandler(eventHandlerType);

            return subscription;
        }

        public IEnumerable<Type> GetHandlersForEvent(string eventName) => _subscriptions[eventName].EventHandlerTypes;

        public Type GetEventType(string eventName) => _subscriptions[eventName].EventType;

        public bool HasHandlerForEvent(string eventName) => _subscriptions.ContainsKey(eventName);

        public void Clear() => _subscriptions.Clear();
    }
}