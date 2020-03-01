using System;

namespace IntegrationBusRabbitMq.Exceptions
{
    public class MissConnectionException : Exception
    {
        public MissConnectionException()
            : base("No connection to RabbitMq is available to performer this action")
        {

        }
    }
}
