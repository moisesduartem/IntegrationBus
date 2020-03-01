using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationBus
{
    public interface ISubscription
    {
        ISubscription And<THandler>();
    }

    public class Subscription : ISubscription
    {
        public IReadOnlyList<Type> EventHandlerTypes => _eventHandlerTypes.ToList();
        private readonly IList<Type> _eventHandlerTypes;
        public Type EventType { get; private set; }

        public Subscription(Type eventType)
        {
            _eventHandlerTypes = new List<Type>();

            EventType = eventType;
        }

        public void AddHandler(Type handlerType) => _eventHandlerTypes.Add(handlerType);

        public ISubscription And<THandler>()
        {
            var handlerType = typeof(THandler);
            AddHandler(handlerType);

            return this;
        }
        
    }
}