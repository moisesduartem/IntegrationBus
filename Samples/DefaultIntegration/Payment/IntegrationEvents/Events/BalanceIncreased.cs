using IntegrationBus;
using System;

namespace Notification.IntegrationEvents.Events
{
    public class BalanceIncreased : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }
        public DateTime DepositMadeAt { get; }
        public DateTime IncreasedAt { get; }        

        public BalanceIncreased(string accountNumber, decimal value, DateTime madeAt)
        {        
            AccountNumber = accountNumber;
            Value = value;
            DepositMadeAt = madeAt;

            IncreasedAt = DateTime.Now;
        }
    }
}
