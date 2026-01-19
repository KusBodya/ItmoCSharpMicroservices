namespace Task21.Options;

public class HttpClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}