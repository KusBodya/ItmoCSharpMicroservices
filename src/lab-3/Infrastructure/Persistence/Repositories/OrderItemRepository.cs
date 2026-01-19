using Application.Ports.Filters;
using Application.Ports.Repo;
using Domain;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public class OrderItemRepository : RepositoryBase, IOrderItemRepository
{
    public OrderItemRepository(NpgsqlDataSource dataSource)
        : base(dataSource) { }

    public async Task<OrderItem> AddAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO order_items (order_id, product_id, order_item_quantity, order_item_deleted)
            VALUES ($1, $2, $3, $4)
            RETURNING order_item_id, order_id, product_id, order_item_quantity, order_item_deleted";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(orderItem.OrderId);
        cmd.Parameters.AddWithValue(orderItem.ProductId);
        cmd.Parameters.AddWithValue(orderItem.OrderItemQuantity);
        cmd.Parameters.AddWithValue(orderItem.OrderItemDeleted);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new OrderItem
        {
            OrderItemId = reader.GetInt64(0),
            OrderId = reader.GetInt64(1),
            ProductId = reader.GetInt64(2),
            OrderItemQuantity = reader.GetInt32(3),
            OrderItemDeleted = reader.GetBoolean(4),
        };
    }

    public async Task<OrderItem?> GetByIdAsync(long orderItemId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
            FROM order_items
            WHERE order_item_id = $1";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(orderItemId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new OrderItem
        {
            OrderItemId = reader.GetInt64(0),
            OrderId = reader.GetInt64(1),
            ProductId = reader.GetInt64(2),
            OrderItemQuantity = reader.GetInt32(3),
            OrderItemDeleted = reader.GetBoolean(4),
        };
    }

    public async Task SoftDeleteAsync(long orderItemId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE order_items
            SET order_item_deleted = TRUE
            WHERE order_item_id = $1";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(orderItemId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderItem>> SearchAsync(
        OrderItemSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string dataSql = """
            SELECT order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
            FROM order_items
            WHERE (cardinality($1::bigint[]) = 0 OR order_id = ANY($1))
              AND (cardinality($2::bigint[]) = 0 OR product_id = ANY($2))
              AND ($3::boolean IS NULL OR order_item_deleted = $3)
            ORDER BY order_item_id
            LIMIT $4 OFFSET $5
            """;

        long[] orderIds = filter.OrderIds?.ToArray() ?? Array.Empty<long>();
        long[] productIds = filter.ProductIds?.ToArray() ?? Array.Empty<long>();
        bool? isDeleted = filter.IsDeleted;

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);

        await using var dataCmd = new NpgsqlCommand(dataSql, conn);
        dataCmd.Parameters.AddWithValue(orderIds);
        dataCmd.Parameters.AddWithValue(productIds);
        dataCmd.Parameters.AddWithValue(isDeleted.HasValue ? isDeleted.Value : DBNull.Value);
        dataCmd.Parameters.AddWithValue(pageSize);
        dataCmd.Parameters.AddWithValue((pageNumber - 1) * pageSize);

        var items = new List<OrderItem>();
        await using NpgsqlDataReader reader = await dataCmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderItem
            {
                OrderItemId = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                ProductId = reader.GetInt64(2),
                OrderItemQuantity = reader.GetInt32(3),
                OrderItemDeleted = reader.GetBoolean(4),
            });
        }

        return items;
    }
}
