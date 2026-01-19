using Application41.DependencyInjection;
using Application41.Ports.Kafka;
using Infrastructure41.DependencyInjection;
using Infrastructure41.Kafka;
using Kafka.Extensions;
using Orders.Kafka.Contracts;
using System.Diagnostics.CodeAnalysis;
using Task21.Manual;
using Task22;
using Task42.Configuration;
using Task42.Infrastructure;
using Task42.Services;

namespace Task42;

public class Program
{
    [SuppressMessage("Design", "CA1506", Justification = "Composition root")]
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureExternalConfiguration(builder);

        builder.Services.AddDatabaseOptions(builder.Configuration)
            .AddDatabaseInfrastructure()
            .AddDatabaseMigrations()
            .AddRepositories()
            .AddApplicationServices();

        builder.Services.AddKafkaProducer<OrderCreationKey, OrderCreationValue>(
            builder.Configuration,
            "Kafka:OrderCreationProducer");

        builder.Services.AddSingleton<IOrderCreationProducer, OrderCreationProducer>();

        builder.Services.AddKafkaConsumer<OrderProcessingKey, OrderProcessingValue, OrderProcessingHandler>(
            builder.Configuration,
            "Kafka:OrderProcessingConsumer");

        builder.Services.AddHostedService<DatabaseMigrationHostedService>();

        builder.Services.AddGrpc(options => options.Interceptors.Add<GrpcExceptionInterceptor>());

        WebApplication app = builder.Build();

        app.MapGrpcService<OrdersGrpcService>();
        app.MapGet("/", () => "gRPC endpoint. Use a gRPC client to call the service.");

        app.Run();
    }

    private static void ConfigureExternalConfiguration(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        IConfigurationSection csSection = configuration.GetSection("ConfigurationService");
        string baseUrl = csSection.GetValue<string>("BaseUrl") ?? "http://localhost:8081";
        int refreshSeconds = csSection.GetValue<int?>("RefreshIntervalSeconds") ?? 60;
        int pageSize = csSection.GetValue<int?>("PageSize") ?? 100;

        var loader = new ManualConfigurationLoader(new StaticHttpClientFactory(new Uri(baseUrl)));
        var provider = new CustomConfigurationProvider();
        var source = new CustomConfigurationSource(provider);
        var service = new CustomConfigurationService(loader, TimeSpan.FromSeconds(refreshSeconds), pageSize);

        if (builder.Configuration is not IConfigurationBuilder configurationBuilder)
        {
            throw new InvalidOperationException("Configuration builder is not available.");
        }

        service.LoadAndApplyConfigurationsAsync(provider);
        configurationBuilder.Add(source);

        builder.Services.AddSingleton(provider);
        builder.Services.AddSingleton<ICustomConfigurationService>(service);
        builder.Services.AddHostedService<ConfigurationBackgroundService>();
    }
}