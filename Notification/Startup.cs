using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Account.IntegrationEvents.Events;
using IntegrationBus;
using IntegrationBusRabbitMq.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.IntegrationEvents.Handlers;

namespace Notification
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

            services.AddTransient<CreateNotificationWhenBalanceIncreasedEventHandler>();
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
            //integrationBus.Subscribe<BalanceIncreased, CreateNotificationWhenBalanceIncreasedEventHandler>(5);

            integrationBus.EnableConsume();
        }
    }
}
