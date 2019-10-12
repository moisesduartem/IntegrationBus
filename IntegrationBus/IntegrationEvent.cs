using System;

namespace IntegrationBus
{
    public abstract class IntegrationEvent
    {
        public Guid EventId { get; }
        public DateTime CreationDate { get; }

        protected IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
    }
}
