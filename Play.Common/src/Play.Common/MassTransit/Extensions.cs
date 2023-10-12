using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Play.Common.Settings;
using MassTransit.Definition;
using System.Reflection;
using System;
using GreenPipes;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.addConsumers(Assembly.GetEntryAssembly());
                configure.UsingRabbitMQ((context, configurator) =>
                {
                    var configuration = context.GetService<IConfiguration>();
                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                    configurator.Host(rabbitMQSettings.Host);
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                    configurator.UseMessageRetry(retryConfigurator =>
                    {
                        retryConfigurator.Interval(3, TimeSpan.fromSeconds(5));
                    });
                });
            });

            services.AddMassTransitHostedService();

            return services;
        }
    }
}