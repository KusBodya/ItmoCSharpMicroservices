using Domain;

namespace Application.Services;

public interface IProductService
{
    Task<Product> CreateAsync(string productName, decimal productPrice, CancellationToken cancellationToken = default);
}
