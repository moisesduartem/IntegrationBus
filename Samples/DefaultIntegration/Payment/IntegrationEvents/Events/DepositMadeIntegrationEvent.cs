using IntegrationBus;
using System;

namespace Account.IntegrationEvents.Events
{
    public class DepositMadeIntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime MadeAt { get; set; }
    }

    public class DepositMade2IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime MadeAt { get; set; }
    }

    public class DepositMade3IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime MadeAt { get; set; }
    }

    public class DepositMade4IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime MadeAt { get; set; }
    }
}
