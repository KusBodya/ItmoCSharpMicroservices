using Task43.Models;

namespace Task43.Controllers.StartOrderControllers.Clients;

public interface IOrdersClient
{
    Task<ProductDto> CreateProductAsync(string name, decimal price, CancellationToken cancellationToken);

    Task<OrderDto> CreateOrderAsync(string createdBy, CancellationToken cancellationToken);

    Task<OrderItemDto> AddItemToOrderAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken);

    Task RemoveItemFromOrderAsync(long orderId, long orderItemId, CancellationToken cancellationToken);

    Task MoveOrderToProcessingAsync(long orderId, CancellationToken cancellationToken);

    Task CancelOrderAsync(long orderId, CancellationToken cancellationToken);

    Task<OrderHistoryResponseDto> GetOrderHistoryAsync(long orderId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
