using Spectre.Console;
using Task23.Abstractions;

namespace Task23.Strategy;

public sealed class UrlImageRenderStrategy : IRenderStrategy
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UrlImageRenderStrategy(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string ContentType => "imageurl";

    public async Task RenderAsync(ContentSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.ImageUrl))
        {
            AnsiConsole.MarkupLine("[yellow]No image URL provided[/]");
            return;
        }

        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            byte[] bytes = await client.GetByteArrayAsync(settings.ImageUrl, cancellationToken);
            CanvasImage image = new CanvasImage(bytes).MaxWidth(settings.MaxImageWidth);
            AnsiConsole.Write(image);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading image from URL: {ex.Message}[/]");
        }
    }
}