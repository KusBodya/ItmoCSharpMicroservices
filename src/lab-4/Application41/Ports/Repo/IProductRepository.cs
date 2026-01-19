using Application41.Ports.Filters;
using Domain41;

namespace Application41.Ports.Repo;

public interface IProductRepository
{
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchAsync(
        ProductSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
