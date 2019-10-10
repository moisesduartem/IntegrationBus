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
       
        public void AddEventSubscription<TEvent, TEventHandler>() 
            where TEvent : IntegrationEvent 
            where TEventHandler : IIntegrationEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var eventHandlerType = typeof(TEventHandler);

            if (!_subscriptions.ContainsKey(eventType.Name))
            {
                var subscription = new Subscription(eventType);
                _subscriptions.Add(eventType.Name, subscription);
            }

            if (_subscriptions[eventType.Name].EventHandlerTypes.Any(h => h == eventHandlerType))
                throw new SubscriptionException(eventHandlerType, eventType.Name);

            _subscriptions[eventType.Name].AddHandler(eventHandlerType);
        }
        
        public IEnumerable<Type> GetHandlersForEvent(string eventName) => _subscriptions[eventName].EventHandlerTypes;

        public Type GetEventType(string eventName) => _subscriptions[eventName].EventType;

        public bool HasHandlerForEvent(string eventName) => _subscriptions.ContainsKey(eventName);

        public void Clear() => _subscriptions.Clear();
    }

    public class Subscription
    {
        public IReadOnlyList<Type> EventHandlerTypes => _eventHandlerTypes.ToList();
        private readonly IList<Type> _eventHandlerTypes;
        public Type EventType { get; private set; }

        public Subscription(Type eventType)
        {
            _eventHandlerTypes = new List<Type>();

            EventType = eventType;
        }

        public void AddHandler(Type handlerType)
        {
            _eventHandlerTypes.Add(handlerType);
        }
    }
}