using Task42.Grpc;
using Task43.Mapper;
using Task43.Models;
using CancelOrderRequest = Task42.Grpc.CancelOrderRequest;
using CreateOrderRequest = Task42.Grpc.CreateOrderRequest;
using CreateProductRequest = Task42.Grpc.CreateProductRequest;
using GetOrderHistoryRequest = Task42.Grpc.GetOrderHistoryRequest;
using MoveToProcessingRequest = Task42.Grpc.MoveToProcessingRequest;

namespace Task43.Controllers.StartOrderControllers.Clients;

public class OrdersGrpcClient(OrdersService.OrdersServiceClient client) : IOrdersClient
{
    public async Task<ProductDto> CreateProductAsync(string name, decimal price, CancellationToken cancellationToken)
    {
        var grpcRequest = new CreateProductRequest
        {
            Name = name,
            Price = (double)price,
        };

        ProductResponse response = await client.CreateProductAsync(grpcRequest, cancellationToken: cancellationToken);
        return OrderMappings.MapProductResponseToDto(response);
    }

    public async Task<OrderDto> CreateOrderAsync(string createdBy, CancellationToken cancellationToken)
    {
        var grpcRequest = new CreateOrderRequest
        {
            CreatedBy = createdBy,
        };

        OrderResponse response = await client.CreateOrderAsync(grpcRequest, cancellationToken: cancellationToken);
        return OrderMappings.MapOrderResponseToDto(response);
    }

    public async Task<OrderItemDto> AddItemToOrderAsync(
        long orderId,
        long productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new AddItemRequest
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };

        OrderItemResponse response =
            await client.AddItemToOrderAsync(grpcRequest, cancellationToken: cancellationToken);
        return OrderMappings.MapOrderItemResponseToDto(response);
    }

    public async Task RemoveItemFromOrderAsync(long orderId, long orderItemId, CancellationToken cancellationToken)
    {
        var grpcRequest = new RemoveItemRequest
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
        };

        await client.RemoveItemFromOrderAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task MoveOrderToProcessingAsync(long orderId, CancellationToken cancellationToken)
    {
        var grpcRequest = new MoveToProcessingRequest
        {
            OrderId = orderId,
        };

        await client.MoveOrderToProcessingAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var grpcRequest = new CancelOrderRequest
        {
            OrderId = orderId,
        };

        await client.CancelOrderAsync(grpcRequest, cancellationToken: cancellationToken);
    }

    public async Task<OrderHistoryResponseDto> GetOrderHistoryAsync(
        long orderId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var grpcRequest = new GetOrderHistoryRequest
        {
            OrderId = orderId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        OrderHistoryResponse response =
            await client.GetOrderHistoryAsync(grpcRequest, cancellationToken: cancellationToken);

        var items = response.Items.Select(OrderMappings.MapHistoryItemToDto).ToList();
        return new OrderHistoryResponseDto(items);
    }
}