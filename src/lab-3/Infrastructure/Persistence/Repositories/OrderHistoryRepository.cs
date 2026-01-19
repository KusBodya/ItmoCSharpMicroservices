using Application.Ports.Filters;
using Application.Ports.Repo;
using Domain;
using Domain.Enums;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Infrastructure.Persistence.Repositories;

public class OrderHistoryRepository : RepositoryBase, IOrderHistoryRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
        : base(dataSource) { }

    public async Task AddAsync(OrderHistoryItem historyItem, CancellationToken cancellationToken = default)
    {
        const string sql = """

                                       INSERT INTO order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                                       VALUES ($1, $2, $3::order_history_item_kind, $4::jsonb)
                           """;

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue(historyItem.OrderId);
        cmd.Parameters.AddWithValue(historyItem.OrderHistoryItemCreatedAt);
        cmd.Parameters.AddWithValue(MapHistoryKindToString(historyItem.OrderHistoryItemKind));

        string json = JsonSerializer.Serialize(historyItem.OrderHistoryItemDataEvent, JsonOptions);
        cmd.Parameters.Add(new NpgsqlParameter { Value = json, NpgsqlDbType = NpgsqlDbType.Jsonb });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderHistoryItem>> SearchAsync(
        OrderHistorySearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string dataSql = """
            SELECT order_history_item_id, order_id, order_history_item_created_at,
                   order_history_item_kind, order_history_item_payload
            FROM order_history
            WHERE (cardinality($1::bigint[]) = 0 OR order_id = ANY($1))
              AND ($2::order_history_item_kind IS NULL OR order_history_item_kind = $2)
            ORDER BY order_history_item_id
            LIMIT $3 OFFSET $4
            """;

        long[] orderIds = filter.OrderIds?.ToArray() ?? Array.Empty<long>();
        string? historyKind = filter.HistoryKind.HasValue ? MapHistoryKindToString(filter.HistoryKind.Value) : null;

        await using NpgsqlConnection conn = await GetConnectionAsync(cancellationToken);

        await using var dataCmd = new NpgsqlCommand(dataSql, conn);
        dataCmd.Parameters.AddWithValue(orderIds);
        dataCmd.Parameters.AddWithValue((object?)historyKind ?? DBNull.Value);
        dataCmd.Parameters.AddWithValue(pageSize);
        dataCmd.Parameters.AddWithValue((pageNumber - 1) * pageSize);

        var items = new List<OrderHistoryItem>();
        await using NpgsqlDataReader reader = await dataCmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            string payloadJson = reader.GetString(4);
            OrderHistoryPayLoad? payload =
                JsonSerializer.Deserialize<OrderHistoryPayLoad>(payloadJson, JsonOptions);

            items.Add(new OrderHistoryItem
            {
                OrderHistoryItemId = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                OrderHistoryItemCreatedAt = reader.GetDateTime(2),
                OrderHistoryItemKind = MapStringToHistoryKind(reader.GetString(3)),
                OrderHistoryItemDataEvent = payload,
            });
        }

        return items;
    }

    private static string MapHistoryKindToString(OrderHistoryItemKind kind)
    {
        return kind switch
        {
            OrderHistoryItemKind.Created => "created",
            OrderHistoryItemKind.ItemAdded => "item_added",
            OrderHistoryItemKind.ItemRemoved => "item_removed",
            OrderHistoryItemKind.StateChanged => "state_changed",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static OrderHistoryItemKind MapStringToHistoryKind(string kind)
    {
        return kind switch
        {
            "created" => OrderHistoryItemKind.Created,
            "item_added" => OrderHistoryItemKind.ItemAdded,
            "item_removed" => OrderHistoryItemKind.ItemRemoved,
            "state_changed" => OrderHistoryItemKind.StateChanged,
            _ => throw new ArgumentException($"Unknown history kind: {kind}"),
        };
    }
}
