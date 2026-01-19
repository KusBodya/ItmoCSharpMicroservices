using Application41.Services;
using Domain41;
using Grpc.Core;
using Task42.Grpc;
using Task42.Services.Mappers;
using DomainOrderHistoryItem = Domain41.OrderHistoryItem;

namespace Task42.Services;

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

        return OrdersGrpcMapper.MapOrderToResponse(order);
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

        return OrdersGrpcMapper.MapOrderItemToResponse(item);
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
            response.Items.Add(OrdersGrpcMapper.MapHistoryItemToProto(item));
        }

        return response;
    }
}