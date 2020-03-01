using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationBusRabbitMq.Exceptions
{
    public class ConnectionConfigurationNullException : Exception
    {
        public ConnectionConfigurationNullException()
            : base("The configuration was not instantiated")
        {

        }
    }
}
