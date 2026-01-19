namespace Task2;

public interface ILibraryOperationHandler
{
    void HandleOperationResult(Guid requestId, byte[] data);

    void HandleOperationError(Guid requestId, Exception exception);
}