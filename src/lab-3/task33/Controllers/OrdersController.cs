using Microsoft.AspNetCore.Mvc;
using Task33.Models;

namespace Task33.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController(IOrdersClient ordersClient, ILogger<OrdersController> logger)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for user: {CreatedBy}", request.CreatedBy);

        OrderDto order = await ordersClient.CreateOrderAsync(
            request.CreatedBy,
            cancellationToken);

        return Ok(order);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItemDto>> AddItemToOrder(
        [FromBody] AddItemToOrderRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Adding item to order {OrderId}: Product {ProductId}, Quantity {Quantity}",
            request.OrderId,
            request.ProductId,
            request.Quantity);

        OrderItemDto item = await ordersClient.AddItemToOrderAsync(
            request.OrderId,
            request.ProductId,
            request.Quantity,
            cancellationToken);

        return Ok(item);
    }

    [HttpDelete("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItemFromOrder(
        [FromBody] RemoveItemFromOrderRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Removing item {OrderItemId} from order {OrderId}",
            request.OrderItemId,
            request.OrderId);

        await ordersClient.RemoveItemFromOrderAsync(
            request.OrderId,
            request.OrderItemId,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("processing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> MoveOrderToProcessing(
        [FromBody] MoveToProcessingRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Moving order {OrderId} to processing", request.OrderId);

        await ordersClient.MoveOrderToProcessingAsync(request.OrderId, cancellationToken);

        return NoContent();
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> CompleteOrder(
        [FromBody] CompleteOrderRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing order {OrderId}", request.OrderId);

        await ordersClient.CompleteOrderAsync(request.OrderId, cancellationToken);

        return NoContent();
    }

    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> CancelOrder(
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling order {OrderId}", request.OrderId);

        await ordersClient.CancelOrderAsync(request.OrderId, cancellationToken);

        return NoContent();
    }

    [HttpGet("{orderId}/history")]
    [ProducesResponseType(typeof(OrderHistoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderHistoryResponseDto>> GetOrderHistory(
        [FromRoute] long orderId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Getting history for order {OrderId}, page {PageNumber}, size {PageSize}",
            orderId,
            pageNumber,
            pageSize);

        OrderHistoryResponseDto history = await ordersClient.GetOrderHistoryAsync(
            orderId,
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(history);
    }
}
