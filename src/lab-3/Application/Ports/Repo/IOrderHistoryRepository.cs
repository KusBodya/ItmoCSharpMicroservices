using Application.Ports.Filters;
using Domain;

namespace Application.Ports.Repo;

public interface IOrderHistoryRepository
{
    Task AddAsync(OrderHistoryItem historyItem, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderHistoryItem>> SearchAsync(
        OrderHistorySearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
