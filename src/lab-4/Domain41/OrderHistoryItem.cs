using Domain41.Enums;

namespace Domain41;

public class OrderHistoryItem
{
    public long OrderHistoryItemId { get; set; }

    public long OrderId { get; set; }

    public DateTime OrderHistoryItemCreatedAt { get; set; }

    public OrderHistoryItemKind OrderHistoryItemKind { get; set; }

    public OrderHistoryPayLoad? OrderHistoryItemDataEvent { get; set; }
}
