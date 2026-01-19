using Application41.Ports.Filters;
using Application41.Ports.Repo;
using Domain41;
using Npgsql;

namespace Infrastructure41.Persistence.Repositories;

public class ProductRepository : RepositoryBase, IProductRepository
{
    public ProductRepository(NpgsqlDataSource dataSource)
        : base(dataSource) { }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = """

                                       INSERT INTO products (product_name, product_price)
                                       VALUES ($1, $2)
                                       RETURNING product_id, product_name, product_price
                           """;

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(product.ProductName);
        cmd.Parameters.AddWithValue(product.ProductPrice);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new Product
        {
            ProductId = reader.GetInt64(0),
            ProductName = reader.GetString(1),
            ProductPrice = reader.GetDecimal(2),
        };
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        ProductSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string dataSql = """
            SELECT product_id, product_name, product_price
            FROM products
            WHERE (cardinality($1::bigint[]) = 0 OR product_id = ANY($1))
              AND ($2::money IS NULL OR product_price >= $2)
              AND ($3::money IS NULL OR product_price <= $3)
              AND ($4::text IS NULL OR product_name ILIKE $4)
            ORDER BY product_id
            LIMIT $5 OFFSET $6
            """;

        long[] productIds = filter.ProductIds?.ToArray() ?? Array.Empty<long>();
        decimal? minPrice = filter.MinPrice;
        decimal? maxPrice = filter.MaxPrice;
        string? namePattern = string.IsNullOrWhiteSpace(filter.NameSubstring)
            ? null
            : $"%{filter.NameSubstring}%";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);

        await using var dataCmd = new NpgsqlCommand(dataSql, conn);
        dataCmd.Parameters.AddWithValue(productIds);
        dataCmd.Parameters.AddWithValue(minPrice.HasValue ? minPrice.Value : DBNull.Value);
        dataCmd.Parameters.AddWithValue(maxPrice.HasValue ? maxPrice.Value : DBNull.Value);
        dataCmd.Parameters.AddWithValue((object?)namePattern ?? DBNull.Value);
        dataCmd.Parameters.AddWithValue(pageSize);
        dataCmd.Parameters.AddWithValue((pageNumber - 1) * pageSize);

        var products = new List<Product>();
        await using NpgsqlDataReader reader = await dataCmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new Product
            {
                ProductId = reader.GetInt64(0),
                ProductName = reader.GetString(1),
                ProductPrice = reader.GetDecimal(2),
            });
        }

        return products;
    }
}
