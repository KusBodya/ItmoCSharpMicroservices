using Task21.Abstractions;
using Task21.Models;

namespace Task22;

public sealed class CustomConfigurationService : ICustomConfigurationService
{
    private readonly IConfigurationLoader _loader;
    private readonly int _pageSize;
    private readonly TimeSpan _refreshInterval;
    private CustomConfigurationProvider? _provider;

    public CustomConfigurationService(IConfigurationLoader loader, TimeSpan refreshInterval, int pageSize = 100)
    {
        _loader = loader;
        _pageSize = pageSize;
        _refreshInterval = refreshInterval;
    }

    public async Task RunAsync(CustomConfigurationProvider provider, CancellationToken cancellationToken = default)
    {
        _provider = provider;

        Dictionary<string, string> initial = await LoadConfigurationsAsync(cancellationToken);
        _provider.UpdateConfigurations(initial);

        using var timer = new PeriodicTimer(_refreshInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                Dictionary<string, string> configs = await LoadConfigurationsAsync(cancellationToken);
                _provider.UpdateConfigurations(configs);
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"Configuration service stopped: {ex.Message}");
        }
    }

    public async Task<Dictionary<string, string>> LoadConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        var configs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        await foreach (ConfigurationModel item in _loader.GetAllConfigurationsAsync(_pageSize, cancellationToken))
        {
            configs[item.Key] = item.Value;
        }

        return configs;
    }
}
