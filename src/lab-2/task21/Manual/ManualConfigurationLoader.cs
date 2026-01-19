using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Task21.Abstractions;
using Task21.Models;

namespace Task21.Manual;

public class ManualConfigurationLoader : IConfigurationLoader
{
    private readonly HttpClient _httpClient;

    public ManualConfigurationLoader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<ConfigurationModel> GetAllConfigurationsAsync(
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? pageToken = null;
        bool hasMorePages = true;

        while (hasMorePages)
        {
            string url = $"/configurations?pageSize={pageSize}";
            if (!string.IsNullOrEmpty(pageToken))
            {
                string encodedToken = Uri.EscapeDataString(pageToken);
                url += $"&pageToken={encodedToken}";
            }

            using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            PagedResponseModel<ConfigurationModel> pagedResponse;
            try
            {
                pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponseModel<ConfigurationModel>>(
                                    cancellationToken: cancellationToken) ??
                                throw new InvalidDataException("Invalid JSON payload from configuration service.");
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Failed to deserialize configuration payload.", ex);
            }

            foreach (ConfigurationModel item in pagedResponse.Items)
            {
                yield return item;
            }

            hasMorePages = pagedResponse.HasNextPage;
            pageToken = pagedResponse.PageToken;
        }
    }
}