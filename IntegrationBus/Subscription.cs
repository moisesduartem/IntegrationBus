using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationBus
{
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