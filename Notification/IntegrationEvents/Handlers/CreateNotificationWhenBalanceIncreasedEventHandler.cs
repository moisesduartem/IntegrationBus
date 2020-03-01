using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Account.IntegrationEvents.Events;
using IntegrationBus;

namespace Notification.IntegrationEvents.Handlers
{
    public class CreateNotificationWhenBalanceIncreasedEventHandler : IIntegrationEventHandler<BalanceIncreased>
    {
        public async Task<bool> Handle(BalanceIncreased @event)
        {
            // Create notification
            var now = DateTime.Now;
            Debug.WriteLine($@"[Notification {@event.Value}] Notification created successfully at {now.ToString("hh:mm:ss")}. 
                               delay from previous {(now - @event.IncreasedAt).Milliseconds}ms and {(now - @event.DepositMadeAt).Milliseconds}ms since created");

            return await Task.FromResult(true);
        }        
    }
}
