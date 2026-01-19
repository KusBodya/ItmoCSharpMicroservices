using Spectre.Console;
using Task23.Abstractions;

namespace Task23.Strategy;

public sealed class FigletRenderStrategy : IRenderStrategy
{
    public string ContentType => "figlettext";

    public Task RenderAsync(ContentSettings settings, CancellationToken cancellationToken = default)
    {
        Color color = settings.FigletColor.ToLowerInvariant() switch
        {
            "red" => Color.Red,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "yellow" => Color.Yellow,
            "cyan" => Color.Cyan1,
            "magenta" => Color.Magenta1,

            _ => Color.White,
        };

        FigletText figlet = new FigletText(settings.FigletText)
            .Centered()
            .Color(color);

        AnsiConsole.Write(figlet);

        return Task.CompletedTask;
    }
}