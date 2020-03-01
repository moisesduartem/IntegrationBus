using IntegrationBus;
using System;

namespace Deposit.IntegrationEvents.Events
{
    public class DepositMadeIntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }

        public DateTime MadeAt { get; }

        public DepositMadeIntegrationEvent(string accountNumber, decimal value, DateTime madeAt)
        {
            AccountNumber = accountNumber;
            Value = value;
            MadeAt = madeAt;
        }
    }

    public class DepositMade2IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }

        public DateTime MadeAt { get; }

        public DepositMade2IntegrationEvent(string accountNumber, decimal value, DateTime madeAt)
        {
            AccountNumber = accountNumber;
            Value = value;
            MadeAt = madeAt;
        }
    }

    public class DepositMade3IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }

        public DateTime MadeAt { get; }

        public DepositMade3IntegrationEvent(string accountNumber, decimal value, DateTime madeAt)
        {
            AccountNumber = accountNumber;
            Value = value;
            MadeAt = madeAt;
        }
    }

    public class DepositMade4IntegrationEvent : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }

        public DateTime MadeAt { get; }

        public DepositMade4IntegrationEvent(string accountNumber, decimal value, DateTime madeAt)
        {
            AccountNumber = accountNumber;
            Value = value;
            MadeAt = madeAt;
        }
    }

    public class BalanceIncreased : IntegrationEvent
    {
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime DepositMadeAt { get; set; }
        public DateTime IncreasedAt { get; set; }
    }
}
