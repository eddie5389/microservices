using Play.Inventario.Service.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.BsonSerialization;
using MongoDB.Bson.BsonSerialization.Serializers;
using MongoDB.Driver;
using Play.Inventario.Service.Repositories;
using Play.Inventario.Service.Settings;
using Play.Inventario.Service.Entities;
using Polly;
using System.Net.Http;
using Polly.TImeout;
using Play.Common.MassTransit;

namespace Play.Inventario.Service
{
    public class Startup
    {
        private ServiceSettings serviceSettings;

        public Startup(Iconfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguracion Configuracion { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            // BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            // serviceSettings = Configuration.getSection(nameof(ServiceSettings)).Get<serviceSettings>();

            services.AddMongo()
                    .AddMongoRepository<InventoryItem>("inventoryitems")
                    .AddMongoRepository<CatalogItem>("catalogitems")
                    .AddMassTransitWithRabbitMq();

            AddCatalogClient(services);

            // services.AddSingleton(ServiceProvider =>
            // {
            //     var mongoDbSettings = Configuration.getSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            //     var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
            //     return mongoClient.GetDatabase(serviceSettings.ServiceName);
            // });

            // services.AddSingleton<IRepository<Item>>(serviceProvider =>
            // {
            //     var database = serviceProvider.GetService<IMongoDatabase>();
            //     return new MongoRepository<Item>(database, "items");
            // });

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
        }

        private static void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();
            
            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7053");
            })
            AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak:(outcome, timespan) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds ...");
                },
                onReset:() =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Closing the circuit ...");
                }
            ))
            .AddPolicyHandler(Policy.TImeOutAsync<HttpResponseMessage>(1));
        }
    }
}