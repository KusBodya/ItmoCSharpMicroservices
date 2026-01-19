using Domain41;
using Domain41.Enums;

namespace Application41.Services;

public class OrderHistoryFactory
{
    public OrderHistoryItem Create(
        long orderId,
        OrderHistoryItemKind kind,
        OrderHistoryPayLoad payload,
        DateTime createdAtUtc)
    {
        return new OrderHistoryItem
        {
            OrderId = orderId,
            OrderHistoryItemCreatedAt = createdAtUtc,
            OrderHistoryItemKind = kind,
            OrderHistoryItemDataEvent = payload,
        };
    }
}
