using Account.IntegrationEvents.Events;
using Account.IntegrationEvents.Handlers;
using IntegrationBus;
using IntegrationBusRabbitMq.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Account
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }      
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureIntegrationBus(Configuration);
            services.ConfigureBusConnectionPersister(Configuration);

            services.AddTransient<IncreaseBalanceWhenDepositMadeIntegrationEventHandler>();
            services.AddTransient<IncreaseBalanceWhenDepositMade2IntegrationEventHandler>();
            services.AddTransient<IncreaseBalanceWhenDepositMade3IntegrationEventHandler>();
            services.AddTransient<IncreaseBalanceWhenDepositMade4IntegrationEventHandler>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseMvc();

            app.ConfigureIntegrationBusSubscribers();
        }
    }

    public static class BusSubscribersConfiguration
    {
        public static void ConfigureIntegrationBusSubscribers(this IApplicationBuilder app)
        {
            var integrationBus = app.ApplicationServices.GetRequiredService<IIntegrationBus>();

            // 1 - 2º
            integrationBus.Subscribe<DepositMadeIntegrationEvent, IncreaseBalanceWhenDepositMadeIntegrationEventHandler>(50);
            // 1 - 3º
            integrationBus.Subscribe<DepositMade2IntegrationEvent, IncreaseBalanceWhenDepositMade2IntegrationEventHandler>(10);
            // 1 - 4º
            integrationBus.Subscribe<DepositMade3IntegrationEvent, IncreaseBalanceWhenDepositMade3IntegrationEventHandler>(1);
            // 1 - 1º
            integrationBus.Subscribe<DepositMade4IntegrationEvent, IncreaseBalanceWhenDepositMade4IntegrationEventHandler>(40);

            integrationBus.EnableConsume();
        }
    }
}

