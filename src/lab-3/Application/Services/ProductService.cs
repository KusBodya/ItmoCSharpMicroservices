using Application.Ports.Repo;
using Domain;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<Product> CreateAsync(
        string productName,
        decimal productPrice,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name must be provided.", nameof(productName));

        if (productPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(productPrice), "Product price must be positive.");

        var product = new Product
        {
            ProductName = productName.Trim(),
            ProductPrice = productPrice,
        };

        return _productRepository.CreateAsync(product, cancellationToken);
    }
}