using Domain41;
using Domain41.PayLoads;
using Google.Protobuf.WellKnownTypes;
using Task42.Grpc;
using DomainOrderHistoryItem = Domain41.OrderHistoryItem;
using DomainOrderHistoryItemKind = Domain41.Enums.OrderHistoryItemKind;
using DomainOrderState = Domain41.Enums.OrderState;
using GrpcOrderHistoryItem = Task42.Grpc.OrderHistoryItem;
using GrpcOrderHistoryKind = Task42.Grpc.OrderHistoryKind;
using GrpcOrderState = Task42.Grpc.OrderState;

namespace Task42.Services.Mappers;

public static class OrdersGrpcMapper
{
    public static OrderResponse MapOrderToResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            State = MapOrderState(order.OrderState),
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(order.OrderCreatedAt, DateTimeKind.Utc)),
            CreatedBy = order.OrderCreatedBy,
        };
    }

    public static OrderItemResponse MapOrderItemToResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            OrderItemId = item.OrderItemId,
            OrderId = item.OrderId,
            ProductId = item.ProductId,
            Quantity = item.OrderItemQuantity,
            Deleted = item.OrderItemDeleted,
        };
    }

    public static GrpcOrderHistoryItem MapHistoryItemToProto(DomainOrderHistoryItem item)
    {
        return new GrpcOrderHistoryItem
        {
            HistoryItemId = item.OrderHistoryItemId,
            OrderId = item.OrderId,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(item.OrderHistoryItemCreatedAt, DateTimeKind.Utc)),
            Kind = MapHistoryKind(item.OrderHistoryItemKind),
            Payload = MapHistoryPayload(item.OrderHistoryItemDataEvent),
        };
    }

    public static OrderHistoryPayload MapHistoryPayload(OrderHistoryPayLoad? payload)
    {
        if (payload is null)
        {
            return new OrderHistoryPayload();
        }

        return payload switch
        {
            OrderCreatedPayLoad created => new OrderHistoryPayload
            {
                Created = new OrderCreatedPayload
                {
                    CreatedBy = created.CreatedBy,
                },
            },
            OrderItemAddedPayLoad added => new OrderHistoryPayload
            {
                ItemAdded = new OrderItemAddedPayload
                {
                    ProductId = added.ProductId,
                    Quantity = added.Quantity,
                },
            },
            OrderItemRemovedPayLoad removed => new OrderHistoryPayload
            {
                ItemRemoved = new OrderItemRemovedPayload
                {
                    ProductId = removed.ProductId,
                },
            },
            OrderStateChangedPayLoad stateChanged => new OrderHistoryPayload
            {
                StateChanged = new OrderStateChangedPayload
                {
                    FromState = stateChanged.FromState,
                    ToState = stateChanged.ToState,
                },
            },
            _ => new OrderHistoryPayload(),
        };
    }

    public static GrpcOrderState MapOrderState(DomainOrderState state)
    {
        return state switch
        {
            DomainOrderState.Created => GrpcOrderState.Created,
            DomainOrderState.Processing => GrpcOrderState.Processing,
            DomainOrderState.Completed => GrpcOrderState.Completed,
            DomainOrderState.Cancelled => GrpcOrderState.Cancelled,
            _ => GrpcOrderState.Unspecified,
        };
    }

    public static GrpcOrderHistoryKind MapHistoryKind(DomainOrderHistoryItemKind kind)
    {
        return kind switch
        {
            DomainOrderHistoryItemKind.Created => GrpcOrderHistoryKind.HistoryCreated,
            DomainOrderHistoryItemKind.ItemAdded => GrpcOrderHistoryKind.HistoryItemAdded,
            DomainOrderHistoryItemKind.ItemRemoved => GrpcOrderHistoryKind.HistoryItemRemoved,
            DomainOrderHistoryItemKind.StateChanged => GrpcOrderHistoryKind.HistoryStateChanged,
            _ => GrpcOrderHistoryKind.HistoryUnspecified,
        };
    }
}