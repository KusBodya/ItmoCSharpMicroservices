using Refit;
using Task21.Models;

namespace Task21.Refit;

public interface IConfigurationApi
{
    [Get("/configurations")]
    Task<PagedResponseModel<ConfigurationModel>> GetConfigurationsPageAsync(
        [Query] int pageSize,
        [Query] string? pageToken = null);
}