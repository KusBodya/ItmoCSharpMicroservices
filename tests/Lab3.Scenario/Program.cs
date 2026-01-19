using Application.DependencyInjection;
using Application.Services;
using Domain;
using Domain.PayLoads;
using Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Task21.Manual;
using Task21.Models;
using Task32.Configuration;

namespace Lab3.Scenario;

internal static class Program
{
    [SuppressMessage("Design", "CA1506", Justification = "Composition root")]
    public static async Task Main(string[] args)
    {
        ConfigurationManager configuration = BuildConfiguration(args);

        ConfigurationServiceOptions configurationServiceOptions = configuration
                                                                      .GetRequiredSection("ConfigurationService")
                                                                      .Get<ConfigurationServiceOptions>()
                                                                  ?? throw new InvalidOperationException(
                                                                      "Failed to load ConfigurationService options.");

        var httpClientFactory = new StaticHttpClientFactory(new Uri(configurationServiceOptions.BaseUrl));
        var loader = new ManualConfigurationLoader(httpClientFactory);

        Dictionary<string, string?> initialConfigurations =
            await EnsureConfigurationServiceAvailableAsync(loader, configurationServiceOptions.PageSize)
                .ConfigureAwait(false);
        ValidateRequiredDatabaseKeys(initialConfigurations);

        configuration.AddInMemoryCollection(initialConfigurations);

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSimpleConsole(options => options.SingleLine = true);
        });

        services.AddDatabaseOptions(configuration)
            .AddDatabaseInfrastructure()
            .AddDatabaseMigrations()
            .AddRepositories()
            .AddApplicationServices();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.RunDatabaseMigrations();

        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IServiceProvider sp = scope.ServiceProvider;

        try
        {
            await ExecuteScenarioAsync(sp).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ILogger logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Scenario");
            logger.LogError(ex, "Scenario execution failed.");
        }

        Console.WriteLine("Scenario finished. Press Enter to exit.");
        Console.ReadLine();
    }

    private static ConfigurationManager BuildConfiguration(string[] args)
    {
        var configuration = new ConfigurationManager();
        configuration.SetBasePath(AppContext.BaseDirectory);
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddEnvironmentVariables(prefix: "TASK31_");
        configuration.AddCommandLine(args);
        return configuration;
    }

    private static async Task<Dictionary<string, string?>> EnsureConfigurationServiceAvailableAsync(
        ManualConfigurationLoader loader,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var configs = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                await foreach (ConfigurationModel item in loader.GetAllConfigurationsAsync(pageSize, cancellationToken)
                                   .ConfigureAwait(false))
                {
                    configs[item.Key] = item.Value;
                }

                return configs;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                Console.WriteLine(
                    $"Configuration service is not available yet (attempt {attempt}/{maxAttempts}): {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("Configuration service is not available.");
    }

    private static void ValidateRequiredDatabaseKeys(Dictionary<string, string?> configurations)
    {
        string[] requiredKeys =
        [
            "Database:Host",
            "Database:Port",
            "Database:Database",
            "Database:Username",
            "Database:Password",
        ];

        string[] missing = requiredKeys
            .Where(key => !configurations.ContainsKey(key) || string.IsNullOrWhiteSpace(configurations[key]))
            .ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing mandatory configuration keys in configuration service: {string.Join(", ", missing)}.");
        }
    }

    private static async Task ExecuteScenarioAsync(IServiceProvider sp)
    {
        IProductService productService = sp.GetRequiredService<IProductService>();
        IOrderService orderService = sp.GetRequiredService<IOrderService>();

        Console.WriteLine("Creating demo products...");
        Product[] products = await CreateProductsAsync(productService).ConfigureAwait(false);
        Console.WriteLine($"Created {products.Length} products.");

        Console.WriteLine("Creating order...");
        Order order = await orderService.CreateOrderAsync("scenario-user").ConfigureAwait(false);
        Console.WriteLine($"Order {order.OrderId} created.");

        Console.WriteLine("Adding products to order...");
        OrderItem firstItem =
            await orderService.AddItemAsync(order.OrderId, products[0].ProductId, 2).ConfigureAwait(false);
        OrderItem secondItem =
            await orderService.AddItemAsync(order.OrderId, products[1].ProductId, 1).ConfigureAwait(false);
        Console.WriteLine("Items added.");

        Console.WriteLine("Removing one item from order...");
        await orderService.RemoveItemAsync(order.OrderId, secondItem.OrderItemId).ConfigureAwait(false);
        Console.WriteLine("Item removed.");

        Console.WriteLine("Moving order to processing...");
        await orderService.MoveToProcessingAsync(order.OrderId).ConfigureAwait(false);
        Console.WriteLine("Order is processing.");

        Console.WriteLine("Completing order...");
        await orderService.CompleteOrderAsync(order.OrderId).ConfigureAwait(false);
        Console.WriteLine("Order completed.");

        Console.WriteLine();
        Console.WriteLine("Order history:");
        await PrintOrderHistoryAsync(orderService, order.OrderId).ConfigureAwait(false);
    }

    private static async Task<Product[]> CreateProductsAsync(IProductService productService)
    {
        var demoProducts = new (string Name, decimal Price)[]
        {
            ("Coffee Canister", 799.90m),
            ("Thermo Mug", 1299.00m),
            ("French Press", 1599.50m),
        };

        var items = new List<Product>();
        foreach ((string name, decimal price) in demoProducts)
        {
            Product product = await productService.CreateAsync(name, price).ConfigureAwait(false);
            items.Add(product);
        }

        return items.ToArray();
    }

    private static async Task PrintOrderHistoryAsync(IOrderService orderService, long orderId)
    {
        IReadOnlyList<OrderHistoryItem> items =
            await orderService.GetHistoryAsync(orderId, pageNumber: 1, pageSize: 50).ConfigureAwait(false);

        foreach (OrderHistoryItem item in items)
        {
            string payloadDescription = item.OrderHistoryItemDataEvent switch
            {
                OrderCreatedPayLoad created => $"created by '{created.CreatedBy}'",
                OrderItemAddedPayLoad added => $"item added productId={added.ProductId} quantity={added.Quantity}",
                OrderItemRemovedPayLoad removed => $"item removed productId={removed.ProductId}",
                OrderStateChangedPayLoad stateChanged => $"state {stateChanged.FromState} -> {stateChanged.ToState}",
                _ => "unknown payload",
            };

            Console.WriteLine(
                $"[{item.OrderHistoryItemCreatedAt:O}] {item.OrderHistoryItemKind}: {payloadDescription}");
        }
    }
}