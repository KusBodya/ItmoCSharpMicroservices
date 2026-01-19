using Task21.Models;

namespace Task21.Abstractions;

public interface IConfigurationLoader
{
    IAsyncEnumerable<ConfigurationModel> GetAllConfigurationsAsync(
        int pageSize,
        CancellationToken cancellationToken);
}