using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Task21.Abstractions;
using Task21.Manual;
using Task21.Options;
using Task21.Refit;

namespace Task21.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddManualConfigurationLoader(
        this IServiceCollection services,
        Action<HttpClientOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddHttpClient<IConfigurationLoader, ManualConfigurationLoader>((sp, client) =>
        {
            HttpClientOptions options = sp.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
    }

    public static void AddRefitConfigurationLoader(
        this IServiceCollection services,
        Action<HttpClientOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddRefitClient<IConfigurationApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                HttpClientOptions options = sp.GetRequiredService<IOptions<HttpClientOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        services.AddScoped<IConfigurationLoader, RefitConfigurationLoader>();
    }
}