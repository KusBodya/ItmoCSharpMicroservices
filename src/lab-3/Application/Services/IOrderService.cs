using Domain;
using OrderHistoryItem = Domain.OrderHistoryItem;

namespace Application.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string createdBy, CancellationToken cancellationToken = default);

    Task<OrderItem> AddItemAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken = default);

    Task RemoveItemAsync(long orderId, long orderItemId, CancellationToken cancellationToken = default);

    Task MoveToProcessingAsync(long orderId, CancellationToken cancellationToken = default);

    Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken = default);

    Task CancelOrderAsync(long orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderHistoryItem>> GetHistoryAsync(
        long orderId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
