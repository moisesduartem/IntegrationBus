using IntegrationBus;

namespace Trigger.IntegrationEvents.Events
{
    #warning MakeADeposit will be transformed in a Command soon, extending IntegrationCommand. This approach is temporary!
    public class MakeADepositIntegrationCommand : IntegrationEvent
    {
        public string AccountNumber { get; }
        public decimal Value { get; }

        public MakeADepositIntegrationCommand(string accountNumber, decimal value)
        {
            AccountNumber = accountNumber;
            Value = value;
        }
    }
}
