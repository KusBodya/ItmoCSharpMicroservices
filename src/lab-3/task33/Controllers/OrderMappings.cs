using Task32.Grpc;
using Task33.Models;

namespace Task33.Controllers;

public static class OrderMappings
{
    public static ProductDto MapProductResponseToDto(ProductResponse response)
    {
        return new ProductDto(
            response.ProductId,
            response.Name,
            response.Price);
    }

    public static OrderDto MapOrderResponseToDto(OrderResponse response)
    {
        return new OrderDto(
            response.OrderId,
            MapOrderState(response.State),
            response.CreatedAt.ToDateTime(),
            response.CreatedBy);
    }

    public static OrderItemDto MapOrderItemResponseToDto(OrderItemResponse response)
    {
        return new OrderItemDto(
            response.OrderItemId,
            response.OrderId,
            response.ProductId,
            response.Quantity,
            response.Deleted);
    }

    public static OrderHistoryItemDto MapHistoryItemToDto(OrderHistoryItem item)
    {
        return new OrderHistoryItemDto(
            item.HistoryItemId,
            item.OrderId,
            item.CreatedAt.ToDateTime(),
            MapHistoryKind(item.Kind),
            MapHistoryPayload(item.Payload));
    }

    private static OrderHistoryPayloadDto? MapHistoryPayload(OrderHistoryPayload? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return payload.PayloadCase switch
        {
            OrderHistoryPayload.PayloadOneofCase.Created => new OrderCreatedPayloadDto(payload.Created.CreatedBy),
            OrderHistoryPayload.PayloadOneofCase.ItemAdded => new OrderItemAddedPayloadDto(
                payload.ItemAdded.ProductId,
                payload.ItemAdded.Quantity),
            OrderHistoryPayload.PayloadOneofCase.ItemRemoved => new OrderItemRemovedPayloadDto(
                payload.ItemRemoved.ProductId),
            OrderHistoryPayload.PayloadOneofCase.StateChanged => new OrderStateChangedPayloadDto(
                payload.StateChanged.FromState,
                payload.StateChanged.ToState),
            OrderHistoryPayload.PayloadOneofCase.None => throw new NotImplementedException(),
            _ => null,
        };
    }

    private static string MapOrderState(OrderState state)
    {
        return state switch
        {
            OrderState.Created => "Created",
            OrderState.Processing => "Processing",
            OrderState.Completed => "Completed",
            OrderState.Cancelled => "Cancelled",
            OrderState.Unspecified => throw new NotImplementedException(),
            _ => "Unspecified",
        };
    }

    private static string MapHistoryKind(OrderHistoryKind kind)
    {
        return kind switch
        {
            OrderHistoryKind.HistoryCreated => "Created",
            OrderHistoryKind.HistoryItemAdded => "ItemAdded",
            OrderHistoryKind.HistoryItemRemoved => "ItemRemoved",
            OrderHistoryKind.HistoryStateChanged => "StateChanged",
            OrderHistoryKind.HistoryUnspecified => throw new NotImplementedException(),
            _ => "Unspecified",
        };
    }
}
