namespace Task42.Configuration;

public class ConfigurationServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8081";

    public int RefreshIntervalSeconds { get; set; } = 60;

    public int PageSize { get; set; } = 100;
}