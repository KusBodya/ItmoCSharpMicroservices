using Microsoft.AspNetCore.Mvc;
using Task43.Controllers.OrderProcessingControllers.Clients;
using Task43.Models;

namespace Task43.Controllers.OrderProcessingControllers;

[ApiController]
[Route("api/order-processing")]
public class OrderProcessingController : ControllerBase
{
    private readonly IProcessingClient _orderProcessingClient;
    private readonly ILogger<OrderProcessingController> _logger;

    public OrderProcessingController(
        IProcessingClient orderProcessingClient,
        ILogger<OrderProcessingController> logger)
    {
        _orderProcessingClient = orderProcessingClient;
        _logger = logger;
    }

    [HttpPost("{orderId}/approve")]
    public async Task<IActionResult> ApproveOrder(
        long orderId,
        [FromBody] ApproveOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderProcessingClient.ApproveOrderAsync(
                orderId,
                request.IsApproved,
                request.ApprovedBy,
                request.FailureReason,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve order {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to approve order" });
        }
    }

    [HttpPost("{orderId}/packing/start")]
    public async Task<IActionResult> StartOrderPacking(
        long orderId,
        [FromBody] StartPackingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderProcessingClient.StartOrderPackingAsync(
                orderId,
                request.PackingBy,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start packing for order {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to start packing" });
        }
    }

    [HttpPost("{orderId}/packing/finish")]
    public async Task<IActionResult> FinishOrderPacking(
        long orderId,
        [FromBody] FinishPackingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderProcessingClient.FinishOrderPackingAsync(
                orderId,
                request.IsSuccessful,
                request.FailureReason,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finish packing for order {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to finish packing" });
        }
    }

    [HttpPost("{orderId}/delivery/start")]
    public async Task<IActionResult> StartOrderDelivery(
        long orderId,
        [FromBody] StartDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderProcessingClient.StartOrderDeliveryAsync(
                orderId,
                request.DeliveredBy,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start delivery for order {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to start delivery" });
        }
    }

    [HttpPost("{orderId}/delivery/finish")]
    public async Task<IActionResult> FinishOrderDelivery(
        long orderId,
        [FromBody] FinishDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _orderProcessingClient.FinishOrderDeliveryAsync(
                orderId,
                request.IsSuccessful,
                request.FailureReason,
                cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to finish delivery for order {OrderId}", orderId);
            return StatusCode(500, new { error = "Failed to finish delivery" });
        }
    }
}