using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IntegrationBus;
using Microsoft.AspNetCore.Mvc;
using Trigger.IntegrationEvents.Events;

namespace Trigger.Controllers
{
    [Route("api/gateway/deposit")]
    [ApiController]
    public class DepositController : ControllerBase
    {
        private readonly IIntegrationBus _integrationBus;

        public DepositController(IIntegrationBus integrationBus) => _integrationBus = integrationBus;

        [HttpPost]
        public IActionResult MakeADeposit(string accountNumber, decimal value)
        {
            Debug.WriteLine($"Deposit request begin...");

            var makeADepositIntegrationCommand = new MakeADepositIntegrationCommand(accountNumber, value);
            _integrationBus.Publish(makeADepositIntegrationCommand);

            return Ok();
        }        
    }
}
