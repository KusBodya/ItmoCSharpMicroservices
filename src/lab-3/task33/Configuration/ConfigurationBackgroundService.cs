using Task22;

namespace Task33.Configuration;

public class ConfigurationBackgroundService : BackgroundService
{
    private readonly ICustomConfigurationService _configurationService;
    private readonly CustomConfigurationProvider _provider;

    public ConfigurationBackgroundService(
        ICustomConfigurationService configurationService,
        CustomConfigurationProvider provider)
    {
        _configurationService = configurationService;
        _provider = provider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _configurationService.LoadAndApplyConfigurationsAsync(_provider, cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _configurationService.RunAsync(_provider, stoppingToken);
    }
}
