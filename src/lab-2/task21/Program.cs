using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task21.Abstractions;
using Task21.Extensions;
using Task21.Models;

namespace Task21;

public static class Program
{
    private static async Task Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices(services =>
        {
            Console.WriteLine("Выберите реализацию:");
            Console.WriteLine("1. Ручная реализация (ManualConfigurationLoader)");
            Console.WriteLine("2. Refit реализация (RefitConfigurationLoader)");
            Console.WriteLine("Ваш выбор (1 или 2):\n ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("\n Используется ручная реализация\n");
                    services.AddManualConfigurationLoader(options =>
                    {
                        options.BaseUrl = "http://localhost:8080";
                        options.TimeoutSeconds = 30;
                    });
                    break;
                case "2":
                    Console.WriteLine("\n Используется Refit реализация\n");
                    services.AddRefitConfigurationLoader(options =>
                    {
                        options.BaseUrl = "http://localhost:8080";
                        options.TimeoutSeconds = 30;
                    });
                    break;
                default:
                    Console.WriteLine("\n Неверный выбор, используется ручная реализация\n");
                    services.AddManualConfigurationLoader(options =>
                    {
                        options.BaseUrl = "http://localhost:8080";
                        options.TimeoutSeconds = 30;
                    });
                    break;
            }
        });

        IHost host = builder.Build();

        IConfigurationLoader loader = host.Services.GetRequiredService<IConfigurationLoader>();

        Console.Write("Введите размер страницы для пагинации (по умолчанию 10): ");
        string? pageSizeInput = Console.ReadLine();
        int pageSize = 10;

        if (!string.IsNullOrWhiteSpace(pageSizeInput) && int.TryParse(pageSizeInput, out int parsedPageSize))
        {
            pageSize = parsedPageSize;
        }

        Console.Write("Сколько конфигураций показать на экране");
        string? displayCountInput = Console.ReadLine();
        int displayCount = 5;

        if (!string.IsNullOrWhiteSpace(displayCountInput) &&
            int.TryParse(displayCountInput, out int parsedDisplayCount))
        {
            displayCount = parsedDisplayCount;
        }

        Console.WriteLine($"\nЗагружаем конфигурации с сервера (размер страницы:{pageSize})\n");

        try
        {
            var configList = new List<ConfigurationModel>();
            await foreach (ConfigurationModel config in loader.GetAllConfigurationsAsync(
                               pageSize,
                               CancellationToken.None))
            {
                configList.Add(config);
            }

            Console.WriteLine($"Получено конфигураций: {configList.Count}\n");

            if (configList.Count > 0)
            {
                int itemsToShow = Math.Min(displayCount, configList.Count);
                Console.WriteLine(
                    $"{(itemsToShow == configList.Count ? "Все" : $"Первые {itemsToShow}")} конфигурации:\n");

                foreach (ConfigurationModel? config in configList.Take(itemsToShow))
                {
                    Console.WriteLine($"  Key:   {config.Key}");
                    Console.WriteLine($"  Value: {config.Value}");
                    Console.WriteLine("  " + new string('-', 50));
                }

                if (configList.Count > itemsToShow)
                {
                    Console.WriteLine($"\n ещё {configList.Count - itemsToShow} конфигураций\n");
                }
            }
            else
            {
                Console.WriteLine("Конфигурации не найдены\n");
                Console.WriteLine("   http://localhost:8080/swagger \n");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка подключения к серверу: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine($"\n Детали:\n{ex}\n");
        }

        Console.WriteLine("\nНажмите Enter для выхода...");
        Console.ReadLine();
    }
}