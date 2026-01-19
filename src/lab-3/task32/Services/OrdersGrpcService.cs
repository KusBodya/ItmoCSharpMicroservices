using Application.Services;
using Domain;
using Domain.PayLoads;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Task32.Grpc;
using DomainOrderHistoryItem = Domain.OrderHistoryItem;
using DomainOrderHistoryItemKind = Domain.Enums.OrderHistoryItemKind;
using DomainOrderState = Domain.Enums.OrderState;
using GrpcOrderHistoryItem = Task32.Grpc.OrderHistoryItem;
using GrpcOrderHistoryKind = Task32.Grpc.OrderHistoryKind;
using GrpcOrderState = Task32.Grpc.OrderState;

namespace Task32.Services;

public class OrdersGrpcService(
    IProductService productService,
    IOrderService orderService) : OrdersService.OrdersServiceBase
{
    public override async Task<ProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        Product product = await productService.CreateAsync(
            request.Name,
            (decimal)request.Price,
            context.CancellationToken);

        return new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.ProductName,
            Price = (double)product.ProductPrice,
        };
    }

    public override async Task<OrderResponse> CreateOrder(
        CreateOrderRequest request,
        ServerCallContext context)
    {
        Order order = await orderService.CreateOrderAsync(
            request.CreatedBy,
            context.CancellationToken);

        return MapOrderToResponse(order);
    }

    public override async Task<OrderItemResponse> AddItemToOrder(
        AddItemRequest request,
        ServerCallContext context)
    {
        OrderItem item = await orderService.AddItemAsync(
            request.OrderId,
            request.ProductId,
            request.Quantity,
            context.CancellationToken);

        return MapOrderItemToResponse(item);
    }

    public override async Task<RemoveItemResponse> RemoveItemFromOrder(
        RemoveItemRequest request,
        ServerCallContext context)
    {
        await orderService.RemoveItemAsync(
            request.OrderId,
            request.OrderItemId,
            context.CancellationToken);

        return new RemoveItemResponse();
    }

    public override async Task<EmptyResponse> MoveOrderToProcessing(
        MoveToProcessingRequest request,
        ServerCallContext context)
    {
        await orderService.MoveToProcessingAsync(
            request.OrderId,
            context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> CompleteOrder(
        CompleteOrderRequest request,
        ServerCallContext context)
    {
        await orderService.CompleteOrderAsync(
            request.OrderId,
            context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<EmptyResponse> CancelOrder(
        CancelOrderRequest request,
        ServerCallContext context)
    {
        await orderService.CancelOrderAsync(
            request.OrderId,
            context.CancellationToken);

        return new EmptyResponse();
    }

    public override async Task<OrderHistoryResponse> GetOrderHistory(
        GetOrderHistoryRequest request,
        ServerCallContext context)
    {
        IReadOnlyList<DomainOrderHistoryItem> items = await orderService.GetHistoryAsync(
            request.OrderId,
            request.PageNumber,
            request.PageSize,
            context.CancellationToken);

        var response = new OrderHistoryResponse();

        foreach (DomainOrderHistoryItem item in items)
        {
            response.Items.Add(MapHistoryItemToProto(item));
        }

        return response;
    }

    private static OrderResponse MapOrderToResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            State = MapOrderState(order.OrderState),
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(order.OrderCreatedAt, DateTimeKind.Utc)),
            CreatedBy = order.OrderCreatedBy,
        };
    }

    private static OrderItemResponse MapOrderItemToResponse(OrderItem item)
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

    private static GrpcOrderHistoryItem MapHistoryItemToProto(DomainOrderHistoryItem item)
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

    private static OrderHistoryPayload MapHistoryPayload(OrderHistoryPayLoad? payload)
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

    private static GrpcOrderState MapOrderState(DomainOrderState state)
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

    private static GrpcOrderHistoryKind MapHistoryKind(DomainOrderHistoryItemKind kind)
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
