using IntegrationBus;
using IntegrationBusRabbitMq.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

namespace IntegrationBusRabbitMq.Extensions
{
    public sealed class IntegrationBusPersisterConfiguration
    {
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int? RetryAttempts { get; set; }
    }

    public static class BusConfiguration
    {
        private const string ConfigurationPersisterSectionName = "IntegrationBusPersisterConfiguration";
        private const string ConfigurationBusSectionName = "IntegrationBusConfiguration";

        private const string DefaultHostNameValue = "localhost";
        private const string DefaultUserNameValue = "guest";
        private const string DefaultPasswordValue = "guest";
        private const string DefaultVirtualHostValue = "/";
        private const int DefaultRetryAttemptsValue = 3;

        private const int DefaultPublishRetryAttemptsValue = 3;
        private const int DefaultConsumeRetryAttemptsValue = 5;
        public const int DefaultConsumeConcurrentsAmountValue = 1;

        public static void ConfigureBusConnectionPersister(this IServiceCollection services, Func<IntegrationBusPersisterConfiguration> configFunc)
        {
            var factory = new ConnectionFactory { DispatchConsumersAsync = false };

            var config = configFunc.Invoke() ?? throw new ConnectionConfigurationNullException();

            factory.HostName = config.HostName ?? DefaultHostNameValue;
            factory.VirtualHost = config.VirtualHost ?? DefaultVirtualHostValue;
            factory.UserName = config.UserName ?? DefaultUserNameValue;
            factory.Password = config.Password ?? DefaultPasswordValue;

            var retryAttemptsValue = config.RetryAttempts ?? DefaultRetryAttemptsValue;

            services.AddSingleton<IBusConnectionPersister>(
                serviceProvider => new BusConnectionPersister(factory, retryAttemptsValue));
        }

        public static void ConfigureBusConnectionPersister(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationPersisterSection = configuration.GetSection(ConfigurationPersisterSectionName);

            var (hostName, userName, virtualHost, password, retryAttempts) = GetEventBusConfigurationStrings(configurationPersisterSection);

            hostName = string.IsNullOrEmpty(hostName) ? DefaultHostNameValue : hostName;

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            };

            factory.UserName = string.IsNullOrEmpty(userName) ? DefaultUserNameValue : userName;
            factory.VirtualHost = string.IsNullOrEmpty(virtualHost) ? DefaultVirtualHostValue : virtualHost;
            factory.Password = string.IsNullOrEmpty(password) ? DefaultPasswordValue : password;

            var retryAttemptsValue = GetRetryAttemptValue(retryAttempts, DefaultRetryAttemptsValue);

            services.AddSingleton<IBusConnectionPersister>(
                serviceProvider => new BusConnectionPersister(factory, retryAttemptsValue));
        }

        private static (string hostName, string userName, string virtualHost, string password, string retryAttempts)
            GetEventBusConfigurationStrings(IConfiguration configuration)
            => (configuration["HostName"],
                configuration["UserName"],
                configuration["VirtualHost"],
                configuration["Password"],
                configuration["ConnectionRetryAttempts"]);

        private static int GetRetryAttemptValue(string configValue, int configDefaultValue)
        {
            var parsed = int.TryParse(configValue, out var retryAttemptsValue);
            return parsed && retryAttemptsValue > 0 ? retryAttemptsValue : configDefaultValue;
        }

        /// <summary>
        /// Injecão do event bus do RabbitMq.
        /// Incluindo o persister do RabbitMq, logger e o gerenciar de eventos e manipuladores em memória.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigureIntegrationBus(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationBusSection = configuration.GetSection(ConfigurationBusSectionName);

            var brokerName = configurationBusSection["BrokerName"];
            if (string.IsNullOrWhiteSpace(brokerName)) throw new Exception("");

            var serviceName = configurationBusSection["ServiceName"];
            if (string.IsNullOrWhiteSpace(serviceName)) throw new Exception("");

            var publishRetryAttempts = configurationBusSection["PublishRetryAttempts"];
            var consumeRetryAttempts = configurationBusSection["ConsumeRetryAttempts"];
            var consumeConcurrentsDefaultAmount = configurationBusSection["ConsumeConcurrentsDefaultAmount"];

            var publishRetryAttemptsValue = GetRetryAttemptValue(publishRetryAttempts, DefaultPublishRetryAttemptsValue);
            var consumeRetryAttemptsValue = GetRetryAttemptValue(consumeRetryAttempts, DefaultConsumeRetryAttemptsValue);
            var consumersDefaultAmountValue = GetRetryAttemptValue(consumeConcurrentsDefaultAmount, DefaultConsumeConcurrentsAmountValue);

            services.AddSingleton<IIntegrationBus, IntegrationBusRabbitMq>(
                serviceProvider => new IntegrationBusRabbitMq(
                    serviceProvider, 
                    (brokerName, serviceName), 
                    (publishRetryAttemptsValue, consumeRetryAttemptsValue),
                    consumersDefaultAmountValue));

            services.AddSingleton<IMemorySubscriptionManager, MemorySubscriptionManager>();
        }
    }
}
