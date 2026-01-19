using Application.Ports.Filters;
using Domain;
using Domain.Enums;

namespace Application.Ports.Repo;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(long orderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> SearchAsync(
        OrderSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
