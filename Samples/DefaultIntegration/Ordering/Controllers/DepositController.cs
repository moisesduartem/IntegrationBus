using Microsoft.AspNetCore.Mvc;
using Deposit.IntegrationEvents.Events;
using IntegrationBus;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Ordering.Controllers
{
    [Route("api/default/deposit")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        private readonly IIntegrationBus _integrationBus;

        public DepositController(IIntegrationBus integrationBus) => _integrationBus = integrationBus;        

        [HttpPost]
        public IActionResult DoDepositViaHttp(string accountNumber, decimal value)
        {
            // Do some input validation logic

            // Do deposit logic and return if successful
            var depositMade = true;

            if (depositMade)
            {
                var l = new List<int>();
                var y = 50_000;
                for (var x = 0; x < y; x++) l.Add(x);

                Parallel.ForEach(l, (i) =>
                {
                    var now = DateTime.Now;
                    var depositMadeIntegrationEvent = new DepositMade4IntegrationEvent(accountNumber, i, now);

                    _integrationBus.Publish(depositMadeIntegrationEvent);
                });

                Parallel.ForEach(l, (i) =>
                {
                    var now = DateTime.Now;
                    var depositMadeIntegrationEvent = new DepositMadeIntegrationEvent(accountNumber, i, now);

                    _integrationBus.Publish(depositMadeIntegrationEvent);
                });

                Parallel.ForEach(l, (i) =>
                {
                    var now = DateTime.Now;
                    var depositMadeIntegrationEvent = new DepositMade2IntegrationEvent(accountNumber, i, now);

                    _integrationBus.Publish(depositMadeIntegrationEvent);
                });

                Parallel.ForEach(l, (i) =>
                {
                    var now = DateTime.Now;
                    var depositMadeIntegrationEvent = new DepositMade3IntegrationEvent(accountNumber, i, now);

                    _integrationBus.Publish(depositMadeIntegrationEvent);
                });

                Parallel.ForEach(l, (i) =>
                {
                    var now = DateTime.Now;
                    var balanceIncreased = new BalanceIncreased
                    {
                        AccountNumber = accountNumber,
                        DepositMadeAt = now,
                        IncreasedAt = now,
                        Value = i
                    };

                    _integrationBus.Publish(balanceIncreased);
                });

                //var now = DateTime.Now;
                //var depositMadeIntegrationEvent = new DepositMadeIntegrationEvent(accountNumber, 1, now);

                //_integrationBus.Publish(depositMadeIntegrationEvent);
                //Debug.WriteLine($@"[Deposit 1] Deposit made successfully at {now.ToString("hh:mm:ss")}. 
                //                    Published in {(DateTime.Now - now).Milliseconds}ms.");

                return Ok(depositMade);
            }

            return UnprocessableEntity($"Error on deposit processing :(");
        }        
    }
}
