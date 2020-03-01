using IntegrationBus;
using System;

namespace Account.IntegrationEvents.Events
{
    public class BalanceIncreased : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime DepositMadeAt { get; set; }
        public DateTime IncreasedAt { get; set; }
    }
}
