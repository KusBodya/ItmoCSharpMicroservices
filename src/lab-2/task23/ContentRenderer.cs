using Microsoft.Extensions.Options;
using Spectre.Console;
using Task23.Abstractions;

namespace Task23;

public class ContentRenderer : IContentRenderer
{
    private readonly IOptionsMonitor<ContentSettings> _options;
    private readonly Dictionary<string, IRenderStrategy> _strategies;

    public ContentRenderer(
        IOptionsMonitor<ContentSettings> options,
        IEnumerable<IRenderStrategy> strategies)
    {
        _options = options;
        _strategies = strategies.ToDictionary(
            s => s.ContentType,
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task RenderAsync(CancellationToken cancellationToken = default)
    {
        ContentSettings settings = _options.CurrentValue;
        AnsiConsole.Clear();

        if (_strategies.TryGetValue(settings.ContentType, out IRenderStrategy? strategy))
        {
            await strategy.RenderAsync(settings, cancellationToken);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Unknown content type![/]");
        }
    }
}