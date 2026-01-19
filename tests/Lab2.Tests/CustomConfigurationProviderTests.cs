using FluentAssertions;
using Moq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Task21.Abstractions;
using Task21.Models;
using Xunit;

namespace Lab2.Tests;

public class CustomConfigurationProviderTests
{
    private const int RefreshIntervalMs = 50;
    private static readonly TimeSpan ObservationTime = TimeSpan.FromMilliseconds(200);

    [Fact]
    public async Task Scenario1_EmptyProvider_AddConfiguration_ShouldContainOneItemAndReload()
    {
        var loaderMock = new Mock<IConfigurationLoader>();
        loaderMock
            .SetupSequence(x => x.GetAllConfigurationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable([]))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "TestValue" }]))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "TestValue" }]));

        using TestableCustomConfigurationProvider provider = CreateProvider(loaderMock);

        provider.Load();

        await WaitUntilAsync(() => provider.TryGet("TestKey", out _));

        provider.TryGet("TestKey", out string? value).Should().BeTrue();
        value.Should().Be("TestValue");
    }

    [Fact]
    public async Task Scenario2_ProviderWithOneItem_AddSameConfiguration_ShouldNotReload()
    {
        var loaderMock = new Mock<IConfigurationLoader>();
        ConfigurationModel[] configs = [new ConfigurationModel { Key = "TestKey", Value = "TestValue" }];

        loaderMock
            .Setup(x => x.GetAllConfigurationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable(configs));

        using TestableCustomConfigurationProvider provider = CreateProvider(loaderMock);

        provider.Load();

        await WaitUntilAsync(() => provider.TryGet("TestKey", out _));

        int reloadsAfterInitial = provider.ReloadCount;
        await EnsureNoAdditionalReloadsAsync(provider, reloadsAfterInitial, ObservationTime);

        provider.ReloadCount.Should().Be(reloadsAfterInitial);
        provider.TryGet("TestKey", out string? value).Should().BeTrue();
        value.Should().Be("TestValue");
    }

    [Fact]
    public async Task Scenario3_ProviderWithOneItem_UpdateValue_ShouldUpdateAndReload()
    {
        var loaderMock = new Mock<IConfigurationLoader>();
        loaderMock
            .SetupSequence(x => x.GetAllConfigurationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "OldValue" }]))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "NewValue" }]))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "NewValue" }]));

        using TestableCustomConfigurationProvider provider = CreateProvider(loaderMock);

        provider.Load();
        await WaitUntilAsync(() => provider.TryGet("TestKey", out string? current) && current == "OldValue");

        int reloadsAfterOldValue = provider.ReloadCount;
        await WaitForAtLeastAsync(provider, reloadsAfterOldValue + 1);

        provider.ReloadCount.Should().BeGreaterThanOrEqualTo(reloadsAfterOldValue + 1);
        provider.TryGet("TestKey", out string? newValue).Should().BeTrue();
        newValue.Should().Be("NewValue");
    }

    [Fact]
    public async Task Scenario4_ProviderWithOneItem_ClearConfigurations_ShouldBecomeEmptyAndReload()
    {
        var loaderMock = new Mock<IConfigurationLoader>();
        loaderMock
            .SetupSequence(x => x.GetAllConfigurationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => ToAsyncEnumerable([new ConfigurationModel { Key = "TestKey", Value = "TestValue" }]))
            .Returns(() => ToAsyncEnumerable([]))
            .Returns(() => ToAsyncEnumerable([]));

        using TestableCustomConfigurationProvider provider = CreateProvider(loaderMock);

        provider.Load();
        await WaitUntilAsync(() => provider.TryGet("TestKey", out _));

        int reloadsAfterInitial = provider.ReloadCount;
        await WaitForAtLeastAsync(provider, reloadsAfterInitial + 1);

        provider.TryGet("TestKey", out _).Should().BeFalse();
        provider.GetChildKeys([], parentPath: null).Should().BeEmpty();
    }

    private static TestableCustomConfigurationProvider CreateProvider(Mock<IConfigurationLoader> loaderMock)
    {
        return new TestableCustomConfigurationProvider(
            loaderMock.Object,
            TimeSpan.FromMilliseconds(RefreshIntervalMs),
            pageSize: 10);
    }

    private static async Task WaitForAtLeastAsync(
        TestableCustomConfigurationProvider provider,
        int minimumReloads)
    {
        var sw = Stopwatch.StartNew();
        while (provider.ReloadCount < minimumReloads)
        {
            if (sw.Elapsed > TimeSpan.FromSeconds(2))
            {
                throw new TimeoutException($"Timed out waiting for at least {minimumReloads} reloads.");
            }

            await Task.Delay(10);
        }
    }

    private static async Task EnsureNoAdditionalReloadsAsync(
        TestableCustomConfigurationProvider provider,
        int expectedReloads,
        TimeSpan observationTime)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < observationTime)
        {
            provider.ReloadCount.Should().Be(expectedReloads);
            await Task.Delay(10);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan? timeout = null)
    {
        TimeSpan limit = timeout ?? TimeSpan.FromSeconds(2);
        var sw = Stopwatch.StartNew();
        while (!predicate())
        {
            if (sw.Elapsed > limit)
                throw new TimeoutException("Timed out waiting for predicate to become true.");

            await Task.Delay(10);
        }
    }

    private static IAsyncEnumerable<ConfigurationModel> ToAsyncEnumerable(IEnumerable<ConfigurationModel> items)
    {
        return Enumerate(items);

        static async IAsyncEnumerable<ConfigurationModel> Enumerate(
            IEnumerable<ConfigurationModel> source,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (ConfigurationModel item in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
                await Task.Yield();
            }
        }
    }
}
