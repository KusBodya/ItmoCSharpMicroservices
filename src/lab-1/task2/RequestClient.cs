using System.Collections.Concurrent;
using Task2.Models;

namespace Task2;

public sealed class RequestClient : IRequestClient, ILibraryOperationHandler
{
    private readonly ILibraryOperationService _service;

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _pending = new();

    public RequestClient(ILibraryOperationService service)
    {
        _service = service;
    }

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var newRequestId = Guid.NewGuid();

        var tcs = new TaskCompletionSource<ResponseModel>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pending.TryAdd(newRequestId, tcs))
            throw new InvalidOperationException();

        await using CancellationTokenRegistration ctr = cancellationToken.Register(() =>
        {
            if (_pending.TryRemove(newRequestId, out TaskCompletionSource<ResponseModel>? toCancel))
                toCancel.TrySetCanceled(cancellationToken);
        });

        try
        {
            _service.BeginOperation(newRequestId, request, cancellationToken);
        }
        catch (Exception ex)
        {
            if (_pending.TryRemove(newRequestId, out TaskCompletionSource<ResponseModel>? toFault))
            {
                toFault.TrySetException(ex);
            }
        }

        return await tcs.Task;
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_pending.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
            tcs.TrySetResult(new ResponseModel(data ?? []));
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (_pending.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs.TrySetException(exception);
        }
    }
}