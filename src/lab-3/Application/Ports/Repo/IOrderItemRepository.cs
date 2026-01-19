using Application.Ports.Filters;
using Domain;

namespace Application.Ports.Repo;

public interface IOrderItemRepository
{
    Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

    Task<OrderItem?> GetByIdAsync(long orderItemId, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(long orderItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderItem>> SearchAsync(
        OrderItemSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
