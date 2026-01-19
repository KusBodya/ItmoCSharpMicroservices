using Domain;
using Domain.Enums;

namespace Application.Services;

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
