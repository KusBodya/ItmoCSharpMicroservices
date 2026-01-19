using Task21.Abstractions;
using Task21.Models;

namespace Task21.Refit;

public class RefitConfigurationLoader : IConfigurationLoader
{
    private readonly IConfigurationApi _api;

    public RefitConfigurationLoader(IConfigurationApi api)
    {
        _api = api;
    }

    public async IAsyncEnumerable<ConfigurationModel> GetAllConfigurationsAsync(
        int pageSize,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        string? pageToken = null;
        bool hasMorePages = true;

        while (hasMorePages)
        {
            PagedResponseModel<ConfigurationModel> pagedResponseModel =
                await _api.GetConfigurationsPageAsync(pageSize, pageToken);

            foreach (ConfigurationModel item in pagedResponseModel.Items)
            {
                yield return item;
            }

            hasMorePages = pagedResponseModel.HasNextPage;
            pageToken = pagedResponseModel.PageToken;
        }
    }
}