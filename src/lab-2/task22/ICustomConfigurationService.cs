namespace Task22;

public interface ICustomConfigurationService
{
    Task<Dictionary<string, string>> LoadConfigurationsAsync(CancellationToken cancellationToken);

    Task RunAsync(CustomConfigurationProvider provider, CancellationToken cancellationToken);
}
