using Task21.Abstractions;
using Task22;

namespace Lab2.Tests;

public sealed class TestableCustomConfigurationProvider : IDisposable
{
    private readonly CustomConfigurationProvider _provider;
    private readonly CustomConfigurationService _service;
    private readonly CancellationTokenSource _cts;
    private readonly Task _runTask;
    private int _reloadCount;

    public TestableCustomConfigurationProvider(IConfigurationLoader loader, TimeSpan refreshInterval, int pageSize)
    {
        _provider = new CustomConfigurationProvider();
        _provider.ConfigurationReloaded += OnConfigurationReloaded;

        _service = new CustomConfigurationService(loader, refreshInterval, pageSize);
        _cts = new CancellationTokenSource();
        _runTask = _service.RunAsync(_provider, _cts.Token);
    }

    public int ReloadCount => Volatile.Read(ref _reloadCount);

    public void Load()
    {
        _provider.Load();
    }

    public bool TryGet(string key, out string? value) => _provider.TryGet(key, out value);

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath) =>
        _provider.GetChildKeys(earlierKeys, parentPath);

    public void Dispose()
    {
        _provider.ConfigurationReloaded -= OnConfigurationReloaded;

        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();

            try
            {
                _runTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
            }
        }

        _cts.Dispose();
    }

    private void OnConfigurationReloaded()
    {
        Interlocked.Increment(ref _reloadCount);
    }
}