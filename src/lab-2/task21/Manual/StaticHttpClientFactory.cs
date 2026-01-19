namespace Task21.Manual;

public sealed class StaticHttpClientFactory(Uri baseAddress) : IHttpClientFactory
{
    public Uri BaseAddress { get; } = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));

    public HttpClient CreateClient(string name)
    {
        return new HttpClient
        {
            BaseAddress = this.BaseAddress,
            Timeout = TimeSpan.FromSeconds(30),
        };
    }
}