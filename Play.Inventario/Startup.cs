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
// using Play.Inventario.Service.Settings;
using Play.Inventario.Service.Entities;
using Play.Common.MassTransit;

namespace Play.Inventario.Service
{
    public class Startup
    {
        private ServiceSettings serviceSettings;
        private const string AllowOriginSetting = "AllowedOrigin";

        public Startup(Iconfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguracion Configuracion { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            // BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            //consumir api plaza mercado
            services.AddHttpClient();

            serviceSettings = Configuration.getSection(nameof(ServiceSettings)).Get<serviceSettings>();

            services.AddMongo()
                    .AddMongoRepository<Item>("items")
                    .AddMassTransitWithRabbitMq();

            // services.AddMassTransit(x =>
            // {
            //     x.UsingRabbitMQ((context, configurator) =>
            //     {
            //         var rabbitMQSettings = Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
            //         configurator.Host(rabbitMQSettings.Host);
            //         configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
            //     });
            // });

            // services.AddMassTransitHostedService();

            services.AddSingleton(ServiceProvider =>
            {
                var mongoDbSettings = Configuration.getSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            services.AddSingleton<IRepository<Item>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<Item>(database, "items");
            });

            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });
        }

        public void AddHttpClient()
        {
            var client = _clientFactory.CreateClient();
            //client.DefaultRequestHeaders.Add("Authorization", $"bearer {your_token_here}");

            //Ingrediente varible dependiendo de la receta
            var mdata = new { Name = "tomato" };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mdata), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://utadeoapi-6dae6e29b5b0.herokuapp.com/api/v1/software-architecture/market-place?ingredient}", content);

            if (response.IsSuccessStatusCode)
            {
                //...
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSetting])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            }
        }
    }
}