using Domain41;

namespace Application41.Services;

public interface IProductService
{
    Task<Product> CreateAsync(string productName, decimal productPrice, CancellationToken cancellationToken = default);
}
