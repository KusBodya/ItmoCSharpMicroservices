using Application41.Ports.Filters;
using Domain41;

namespace Application41.Ports.Repo;

public interface IOrderHistoryRepository
{
    Task AddAsync(OrderHistoryItem historyItem, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderHistoryItem>> SearchAsync(
        OrderHistorySearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}