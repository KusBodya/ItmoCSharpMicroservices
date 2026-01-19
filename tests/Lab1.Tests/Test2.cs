using FluentAssertions;
using NSubstitute;
using Task2;
using Task2.Models;
using Xunit;

namespace Lab1.Tests;

public class Test2
{
    [Fact]
    public async Task Scenario1SendAsyncThenResultAwaitReturnsThatResult()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        var request = new RequestModel("op", [1, 2, 3]);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var began = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
        Guid capturedId = Guid.Empty;

        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                capturedId = ci.ArgAt<Guid>(0);
                began.TrySetResult(capturedId);
            });

        Task<ResponseModel> task = client.SendAsync(request, cts.Token);

        await began.Task;

        byte[] expected = "\t\t\t"u8.ToArray();
        client.HandleOperationResult(capturedId, expected);

        ResponseModel response = await task;
        response.Data.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Scenario2SendAsyncThenErrorAwaitThrowsSameException()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var began = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
        Guid capturedId = Guid.Empty;

        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                capturedId = ci.ArgAt<Guid>(0);
                began.TrySetResult(capturedId);
            });

        Task<ResponseModel> task = client.SendAsync(new RequestModel("x", Array.Empty<byte>()), cts.Token);

        await began.Task;

        var ex = new InvalidOperationException("boom");
        client.HandleOperationError(capturedId, ex);

        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Equal("boom", thrown.Message);
    }

    [Fact]
    public async Task Scenario3AlreadyCanceledTokenAwaitThrowsTaskCanceled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Task<ResponseModel> task = client.SendAsync(new RequestModel("x", Array.Empty<byte>()), cts.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Fact]
    public async Task Scenario4CancelLaterAwaitThrowsTaskCanceled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        using var cts = new CancellationTokenSource();

        Task<ResponseModel> task = client.SendAsync(new RequestModel("x", Array.Empty<byte>()), cts.Token);

        cts.CancelAfter(10);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Fact]
    public async Task Scenario5ResultInsideBeginOperationReturnsThatResult()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                Guid id = ci.ArgAt<Guid>(0);
                client.HandleOperationResult(id, new byte[] { 1, 2, 3 });
            });

        ResponseModel result =
            await client.SendAsync(new RequestModel("x", Array.Empty<byte>()), CancellationToken.None);

        result.Data.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Scenario6ErrorInsideBeginOperationAwaitThrowsSameException()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(ci =>
            {
                Guid id = ci.ArgAt<Guid>(0);
                client.HandleOperationError(id, new InvalidOperationException("ERR"));
            });

        Task<ResponseModel> task = client.SendAsync(new RequestModel("x", Array.Empty<byte>()), CancellationToken.None);

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Equal("ERR", ex.Message);
    }

    [Fact]
    public async Task Scenario7CancelInsideBeginOperationAwaitThrowsTaskCanceled()
    {
        ILibraryOperationService service = Substitute.For<ILibraryOperationService>();
        var client = new RequestClient(service);

        using var cts = new CancellationTokenSource();

        service.When(s => s.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(_ => { cts.Cancel(); });

        Task<ResponseModel> task = client.SendAsync(new RequestModel("x", Array.Empty<byte>()), cts.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }
}