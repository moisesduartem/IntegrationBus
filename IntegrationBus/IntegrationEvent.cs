using System;

namespace IntegrationBus
{
    public class IntegrationEvent
    {
        public Guid EventId { get; }
        public DateTime CreationDate { get; }

        public IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
    }
}
