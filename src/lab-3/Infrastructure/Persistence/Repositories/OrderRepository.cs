using Application.Ports.Filters;
using Application.Ports.Repo;
using Domain;
using Domain.Enums;
using Npgsql;

#pragma warning disable CA2100
namespace Infrastructure.Persistence.Repositories;

public class OrderRepository : RepositoryBase, IOrderRepository
{
    public OrderRepository(NpgsqlDataSource dataSource)
        : base(dataSource) { }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO orders (order_state, order_created_at, order_created_by)
            VALUES ($1::order_state, $2, $3)
            RETURNING order_id, order_state, order_created_at, order_created_by";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(MapOrderStateToString(order.OrderState));
        cmd.Parameters.AddWithValue(order.OrderCreatedAt);
        cmd.Parameters.AddWithValue(order.OrderCreatedBy);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new Order
        {
            OrderId = reader.GetInt64(0),
            OrderState = MapStringToOrderState(reader.GetString(1)),
            OrderCreatedAt = reader.GetDateTime(2),
            OrderCreatedBy = reader.GetString(3),
        };
    }

    public async Task UpdateStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE orders
            SET order_state = $1::order_state
            WHERE order_id = $2";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(MapOrderStateToString(newState));
        cmd.Parameters.AddWithValue(orderId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT order_id, order_state, order_created_at, order_created_by
            FROM orders
            WHERE order_id = $1";

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(orderId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new Order
        {
            OrderId = reader.GetInt64(0),
            OrderState = MapStringToOrderState(reader.GetString(1)),
            OrderCreatedAt = reader.GetDateTime(2),
            OrderCreatedBy = reader.GetString(3),
        };
    }

    public async Task<IReadOnlyList<Order>> SearchAsync(
        OrderSearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string dataSql = """
            SELECT order_id, order_state, order_created_at, order_created_by
            FROM orders
            WHERE (cardinality($1::bigint[]) = 0 OR order_id = ANY($1))
              AND ($2::order_state IS NULL OR order_state = $2)
              AND ($3::text IS NULL OR order_created_by = $3)
            ORDER BY order_id
            LIMIT $4 OFFSET $5
            """;

        long[] orderIds = filter.OrderIds?.ToArray() ?? Array.Empty<long>();
        string? state = filter.State.HasValue ? MapOrderStateToString(filter.State.Value) : null;
        string? author = string.IsNullOrWhiteSpace(filter.Author) ? null : filter.Author;

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);

        await using var dataCmd = new NpgsqlCommand(dataSql, conn);
        dataCmd.Parameters.AddWithValue(orderIds);
        dataCmd.Parameters.AddWithValue((object?)state ?? DBNull.Value);
        dataCmd.Parameters.AddWithValue((object?)author ?? DBNull.Value);
        dataCmd.Parameters.AddWithValue(pageSize);
        dataCmd.Parameters.AddWithValue((pageNumber - 1) * pageSize);

        var orders = new List<Order>();
        await using NpgsqlDataReader reader = await dataCmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new Order
            {
                OrderId = reader.GetInt64(0),
                OrderState = MapStringToOrderState(reader.GetString(1)),
                OrderCreatedAt = reader.GetDateTime(2),
                OrderCreatedBy = reader.GetString(3),
            });
        }

        return orders;
    }

    private static string MapOrderStateToString(OrderState state) => state switch
    {
        OrderState.Created => "created",
        OrderState.Processing => "processing",
        OrderState.Completed => "completed",
        OrderState.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };

    private static OrderState MapStringToOrderState(string state) => state switch
    {
        "created" => OrderState.Created,
        "processing" => OrderState.Processing,
        "completed" => OrderState.Completed,
        "cancelled" => OrderState.Cancelled,
        _ => throw new ArgumentException($"Unknown order state: {state}"),
    };
}
