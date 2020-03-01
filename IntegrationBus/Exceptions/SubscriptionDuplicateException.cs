using System;

namespace IntegrationBus.Exceptions
{
    public class SubscriptionDuplicateException : Exception
    {
        public SubscriptionDuplicateException(Type eventHandlerType, string eventName) 
            : base($"Há duplicidade no registro do manipulador {eventHandlerType.Name} para o event '{eventName}'")
        {
            
        }
    }
}
