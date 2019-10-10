using System;

namespace IntegrationBus.Exceptions
{
    public class SubscriptionException : Exception
    {
        public SubscriptionException(Type eventHandlerType, string eventName) 
            : base($"Há duplicidade no registro do manipulador {eventHandlerType.Name} para o event '{eventName}'")
        {
            
        }
    }
}
