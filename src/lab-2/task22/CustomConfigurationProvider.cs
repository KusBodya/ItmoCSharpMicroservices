using Microsoft.Extensions.Configuration;

namespace Task22;

public class CustomConfigurationProvider : ConfigurationProvider
{
    private Dictionary<string, string?>? _previousData;

    private static Dictionary<string, string?> ToProviderData(Dictionary<string, string> source)
    {
        return source.ToDictionary(
            item => item.Key,
            string? (item) => item.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    public event Action? ConfigurationReloaded;

    public override void Load()
    {
        if (_previousData is null)
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        Data = new Dictionary<string, string?>(_previousData, StringComparer.OrdinalIgnoreCase);
    }

    protected internal void UpdateConfigurations(Dictionary<string, string> newConfigurations)
    {
        if (!HasChanges(newConfigurations))
            return;

        Data = ToProviderData(newConfigurations);
        _previousData = new Dictionary<string, string?>(Data, StringComparer.OrdinalIgnoreCase);

        OnReload();
        ConfigurationReloaded?.Invoke();
    }

    private bool HasChanges(Dictionary<string, string> newConfigurations)
    {
        if (_previousData is null) return true;
        if (_previousData.Count != newConfigurations.Count) return true;

        foreach (KeyValuePair<string, string> kv in newConfigurations)
        {
            if (!_previousData.TryGetValue(kv.Key, out string? oldValue) || oldValue != kv.Value)
                return true;
        }

        return false;
    }
}