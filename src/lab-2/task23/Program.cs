using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Task23.Abstractions;
using Task23.Strategy;

namespace Task23;

public class Program
{
    public static async Task Main(string[] args)
    {
        const string ConfigPath = @"C:\Users\kolpi\RiderProjects\KusBodya\src\task23\filesJ\appsettings.json";

        if (!File.Exists(ConfigPath))
        {
            AnsiConsole.MarkupLine($"[red]Config file not found:[/] {ConfigPath}");
            return;
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(ConfigPath, optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton(configuration);

        services
            .AddOptions<ContentSettings>()
            .Bind(configuration.GetSection("ContentSettings"))
            .Validate(s => !string.IsNullOrWhiteSpace(s.ContentType), "ContentType is required")
            .Validate(s => s.MaxImageWidth > 0, "MaxImageWidth must be > 0")
            .PostConfigure(s =>
            {
                s.ContentType = s.ContentType?.Trim() ?? "figlettext";
                s.FigletColor ??= "white";
                s.FigletText ??= "Hello";
            });

        services.AddSingleton<IRenderStrategy, FigletRenderStrategy>();
        services.AddSingleton<IRenderStrategy, Base64ImageRenderStrategy>();
        services.AddHttpClient();
        services.AddSingleton<IRenderStrategy, UrlImageRenderStrategy>();
        services.AddSingleton<IContentRenderer, ContentRenderer>();

        await using ServiceProvider provider = services.BuildServiceProvider();

        IOptionsMonitor<ContentSettings> optionsMonitor =
            provider.GetRequiredService<IOptionsMonitor<ContentSettings>>();
        IContentRenderer renderer = provider.GetRequiredService<IContentRenderer>();

        await renderer.RenderAsync();
        AnsiConsole.MarkupLine("\n[dim]Monitoring configuration changes... Press Ctrl+C to exit.[/]");

        using IDisposable? reg = optionsMonitor.OnChange(async void (_, __) =>
        {
            AnsiConsole.MarkupLine("[cyan]Configuration changed! Updating display...[/]");
            try
            {
                await renderer.RenderAsync();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error rendering: {ex.Message}[/]");
            }
        });

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await Task.Delay(Timeout.Infinite, cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }
}