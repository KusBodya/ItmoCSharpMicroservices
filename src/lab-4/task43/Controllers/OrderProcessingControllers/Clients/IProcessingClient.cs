namespace Task43.Controllers.OrderProcessingControllers.Clients;

public interface IProcessingClient : IDisposable
{
    Task ApproveOrderAsync(
        long orderId,
        bool isApproved,
        string approvedBy,
        string? failureReason,
        CancellationToken cancellationToken);

    Task StartOrderPackingAsync(
        long orderId,
        string packingBy,
        CancellationToken cancellationToken);

    Task FinishOrderPackingAsync(
        long orderId,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken);

    Task StartOrderDeliveryAsync(
        long orderId,
        string deliveredBy,
        CancellationToken cancellationToken);

    Task FinishOrderDeliveryAsync(
        long orderId,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken);
}