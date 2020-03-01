using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Account.IntegrationEvents.Events;
using IntegrationBus;
using Notification.IntegrationEvents.Events;

namespace Account.IntegrationEvents.Handlers
{
    public class IncreaseBalanceWhenDepositMadeIntegrationEventHandler : IIntegrationEventHandler<DepositMadeIntegrationEvent>
    {
        private readonly IIntegrationBus _integrationBus;
        public IncreaseBalanceWhenDepositMadeIntegrationEventHandler(IIntegrationBus integrationBus)
        {
            _integrationBus = integrationBus;
        }

        public async Task<bool> Handle(DepositMadeIntegrationEvent @event)
        {
            // Do something with account number and value
            var balanceIncreased = true;

            if (balanceIncreased)
            {
                Debug.WriteLine($@"[Account {@event.Value}] HANDLER 11111111");
                //var now = DateTime.Now;
                //var balanceIncreasedIntegrationEvent = new BalanceIncreased(@event.AccountNumber, @event.Value, @event.MadeAt);

                //_integrationBus.Publish(balanceIncreasedIntegrationEvent);
                //Debug.WriteLine($@"[Account {@event.Value}] Balance increased successfully at {now.ToString("hh:mm:ss")}. 
                //                Published in {(DateTime.Now - now).Milliseconds}ms and {(now - @event.MadeAt).Milliseconds}ms since created");
            }

            return await Task.FromResult(true);
        }        
    }

    public class IncreaseBalanceWhenDepositMade2IntegrationEventHandler : IIntegrationEventHandler<DepositMade2IntegrationEvent>
    {
        private readonly IIntegrationBus _integrationBus;
        public IncreaseBalanceWhenDepositMade2IntegrationEventHandler(IIntegrationBus integrationBus)
        {
            _integrationBus = integrationBus;
        }

        public async Task<bool> Handle(DepositMade2IntegrationEvent @event)
        {
            // Do something with account number and value
            var balanceIncreased = true;

            if (balanceIncreased)
            {
                Debug.WriteLine($@"[Account {@event.Value}] HANDLER 22222222");
                //var now = DateTime.Now;
                //var balanceIncreasedIntegrationEvent = new BalanceIncreased(@event.AccountNumber, @event.Value, @event.MadeAt);

                //_integrationBus.Publish(balanceIncreasedIntegrationEvent);
                //Debug.WriteLine($@"[Account {@event.Value}] Balance increased successfully at {now.ToString("hh:mm:ss")}. 
                //                Published in {(DateTime.Now - now).Milliseconds}ms and {(now - @event.MadeAt).Milliseconds}ms since created");
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> HandleCompensatory(DepositMadeIntegrationEvent @event)
        {
            Debug.WriteLine($@"Handling compensatory");
            return await Task.FromResult(true);
        }
    }

    public class IncreaseBalanceWhenDepositMade3IntegrationEventHandler : IIntegrationEventHandler<DepositMade3IntegrationEvent>
    {
        private readonly IIntegrationBus _integrationBus;
        public IncreaseBalanceWhenDepositMade3IntegrationEventHandler(IIntegrationBus integrationBus)
        {
            _integrationBus = integrationBus;
        }

        public async Task<bool> Handle(DepositMade3IntegrationEvent @event)
        {
            // Do something with account number and value
            var balanceIncreased = true;

            if (balanceIncreased)
            {
                Debug.WriteLine($@"[Account {@event.Value}] HANDLER 33333333");
                //var now = DateTime.Now;
                //var balanceIncreasedIntegrationEvent = new BalanceIncreased(@event.AccountNumber, @event.Value, @event.MadeAt);

                //_integrationBus.Publish(balanceIncreasedIntegrationEvent);
                //Debug.WriteLine($@"[Account {@event.Value}] Balance increased successfully at {now.ToString("hh:mm:ss")}. 
                //                Published in {(DateTime.Now - now).Milliseconds}ms and {(now - @event.MadeAt).Milliseconds}ms since created");
            }

            return await Task.FromResult(true);
        }
    }

    public class IncreaseBalanceWhenDepositMade4IntegrationEventHandler : IIntegrationEventHandler<DepositMade4IntegrationEvent>
    {
        private readonly IIntegrationBus _integrationBus;
        public IncreaseBalanceWhenDepositMade4IntegrationEventHandler(IIntegrationBus integrationBus)
        {
            _integrationBus = integrationBus;
        }

        public async Task<bool> Handle(DepositMade4IntegrationEvent @event)
        {
            // Do something with account number and value
            var balanceIncreased = true;

            if (balanceIncreased)
            {
                Debug.WriteLine($@"[Account {@event.Value}] HANDLER 44444444");
                //var now = DateTime.Now;
                //var balanceIncreasedIntegrationEvent = new BalanceIncreased(@event.AccountNumber, @event.Value, @event.MadeAt);

                //_integrationBus.Publish(balanceIncreasedIntegrationEvent);
                //Debug.WriteLine($@"[Account {@event.Value}] Balance increased successfully at {now.ToString("hh:mm:ss")}. 
                //                Published in {(DateTime.Now - now).Milliseconds}ms and {(now - @event.MadeAt).Milliseconds}ms since created");
            }

            return await Task.FromResult(true);
        }
    }
}
