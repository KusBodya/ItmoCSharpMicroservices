using Application.Ports.Filters;
using Domain;

namespace Application.Ports.Repo;

public interface IProductRepository
{
    Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> SearchAsync(
        ProductSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
