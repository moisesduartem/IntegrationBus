using IntegrationBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace IntegrationBusRabbitMq.Extensions
{
    public static class BusConfiguration
    {
        public const int DefaultRetryAttempts = 3;

        public static void ConfigureBusConnectionPersister(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBusConnectionPersister>(persister =>
            {
                var (connection, username, password, retryAttempts) = GetEventBusConfigurationStrings(configuration);

                var factory = new ConnectionFactory
                {
                    HostName = connection,
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(username))
                    factory.UserName = username;

                if (!string.IsNullOrEmpty(password))
                    factory.Password = password;

                var retryAttemptsValue = GetRetryAttemptValue(retryAttempts);

                return new BusConnectionPersister(factory, retryAttemptsValue);

            });
        }

        private static (string connection, string username, string password, string retryAttempts)
            GetEventBusConfigurationStrings(IConfiguration configuration)
            => (configuration["IntegrationBusConnection"],
                configuration["IntegrationBusUserName"],
                configuration["IntegrationBusPassword"],
                configuration["IntegrationBusConnectionRetryAttempts"]);

        private static int GetRetryAttemptValue(string configValue)
        {
            var parsed = int.TryParse(configValue, out var retryAttemptsValue);
            return parsed ? retryAttemptsValue : DefaultRetryAttempts;
        }

        /// <summary>
        /// Injecão do event bus do RabbitMq.
        /// Incluindo o persister do RabbitMq, logger e o gerenciar de eventos e manipuladores em memória.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureIntegrationBus(this IServiceCollection services, IConfiguration configuration)
        {
            var brokerName = configuration["IntegrationBusConfigurationBrokerName"];
            var queueName = configuration["IntegrationBusConfigurationQueueName"];

            var publishRetryAttempts = configuration["IntegrationBusPublishRetryAttempts"];
            var consumeRetryAttempts = configuration["IntegrationBusConsumeRetryAttempts"];

            var publishRetryAttemptsValue = GetRetryAttemptValue(publishRetryAttempts);
            var consumeRetryAttemptsValue = GetRetryAttemptValue(consumeRetryAttempts);

            services.AddSingleton<IIntegrationBus, IntegrationBusRabbitMq>(
                serviceProvider => new IntegrationBusRabbitMq(
                    serviceProvider, 
                    (brokerName, queueName), 
                    (publishRetryAttemptsValue, consumeRetryAttemptsValue)));

            services.AddSingleton<IMemorySubscriptionManager, MemorySubscriptionManager>();
        }
    }
}
