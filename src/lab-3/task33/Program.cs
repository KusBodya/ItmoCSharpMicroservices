using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Task21.Manual;
using Task22;
using Task32.Grpc;
using Task33.Configuration;
using Task33.Controllers;
using Task33.Middleware;
using Task33.Options;

namespace Task33;

public class Program
{
    [SuppressMessage("Design", "CA1506", Justification = "Composition root")]
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigureExternalConfiguration(builder);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Orders API Gateway",
                    Version = "v1",
                    Description = "HTTP Gateway for Orders gRPC Service",
                });

            options.UseAllOfForInheritance();
            options.UseOneOfForPolymorphism();
            options.SelectDiscriminatorNameUsing(_ => "$type");
        });

        builder.Services.Configure<GrpcServiceOptions>(
            builder.Configuration.GetSection(GrpcServiceOptions.SectionName));

        builder.Services.AddGrpcClient<OrdersService.OrdersServiceClient>((sp, options) =>
        {
            GrpcServiceOptions grpcOptions = sp.GetRequiredService<IOptionsMonitor<GrpcServiceOptions>>().CurrentValue;
            if (string.IsNullOrWhiteSpace(grpcOptions.Url))
                throw new InvalidOperationException("GrpcService:Url configuration is missing.");

            options.Address = new Uri(grpcOptions.Url);
        });

        builder.Services.AddScoped<IOrdersClient, OrdersGrpcClient>();

        WebApplication app = builder.Build();

        app.UseMiddleware<GrpcExceptionMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }

    private static void ConfigureExternalConfiguration(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        IConfigurationSection csSection = configuration.GetSection("ConfigurationService");
        string baseUrl = csSection.GetValue<string>("BaseUrl") ?? "http://localhost:8080";
        int refreshSeconds = csSection.GetValue<int?>("RefreshIntervalSeconds") ?? 60;
        int pageSize = csSection.GetValue<int?>("PageSize") ?? 100;

        var loader = new ManualConfigurationLoader(new StaticHttpClientFactory(new Uri(baseUrl)));
        var provider = new CustomConfigurationProvider();
        var source = new CustomConfigurationSource(provider);
        var service = new CustomConfigurationService(loader, TimeSpan.FromSeconds(refreshSeconds), pageSize);

        if (builder.Configuration is not IConfigurationBuilder configurationBuilder)
            throw new InvalidOperationException("Configuration builder is not available.");

        configurationBuilder.Add(source);

        builder.Services.AddSingleton(provider);
        builder.Services.AddSingleton<ICustomConfigurationService>(service);
        builder.Services.AddHostedService<ConfigurationBackgroundService>();
    }
}
