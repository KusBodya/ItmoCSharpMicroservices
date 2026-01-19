using Spectre.Console;
using Task23.Abstractions;

namespace Task23.Strategy;

public sealed class Base64ImageRenderStrategy : IRenderStrategy
{
    public string ContentType => "imagebase64";

    public Task RenderAsync(ContentSettings settings, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(settings.ImageBase64))
        {
            AnsiConsole.MarkupLine("[yellow]No Base64 image data provided[/]");
            return Task.CompletedTask;
        }

        try
        {
            byte[] bytes = Convert.FromBase64String(settings.ImageBase64);
            CanvasImage image = new CanvasImage(bytes).MaxWidth(settings.MaxImageWidth);
            AnsiConsole.Write(image);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading Base64 image: {ex.Message}[/]");
        }

        return Task.CompletedTask;
    }
}