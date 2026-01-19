using Application41.Ports.Repo;
using Application41.Services;
using Domain41;
using Domain41.Enums;
using Domain41.PayLoads;
using Kafka.Consumer;
using Microsoft.Extensions.Logging;
using Orders.Kafka.Contracts;

namespace Infrastructure41.Kafka;

public class OrderProcessingHandler : IMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IOrderService _orderService;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly OrderHistoryFactory _historyFactory;
    private readonly ILogger<OrderProcessingHandler> _logger;

    public OrderProcessingHandler(
        IOrderService orderService,
        IOrderHistoryRepository orderHistoryRepository,
        OrderHistoryFactory historyFactory,
        ILogger<OrderProcessingHandler> logger)
    {
        _orderService = orderService;
        _orderHistoryRepository = orderHistoryRepository;
        _historyFactory = historyFactory;
        _logger = logger;
    }

    public async Task HandleAsync(
        IReadOnlyCollection<MessageContext<OrderProcessingKey, OrderProcessingValue>> messages,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing batch of {Count} messages", messages.Count);

        foreach (MessageContext<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process message for order {OrderId} at offset {Offset}",
                    message.Key.OrderId,
                    message.Offset);
                throw;
            }
        }

        _logger.LogInformation("Successfully processed batch of {Count} messages", messages.Count);
    }

    private Task ProcessMessageAsync(
        MessageContext<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        long orderId = message.Key.OrderId;

        switch (message.Value.EventCase)
        {
            case OrderProcessingValue.EventOneofCase.ApprovalReceived:
                return HandleApprovalReceivedAsync(orderId, message.Value.ApprovalReceived, cancellationToken);

            case OrderProcessingValue.EventOneofCase.PackingStarted:
                return HandlePackingStartedAsync(orderId, message.Value.PackingStarted, cancellationToken);

            case OrderProcessingValue.EventOneofCase.PackingFinished:
                return HandlePackingFinishedAsync(orderId, message.Value.PackingFinished, cancellationToken);

            case OrderProcessingValue.EventOneofCase.DeliveryStarted:
                return HandleDeliveryStartedAsync(orderId, message.Value.DeliveryStarted, cancellationToken);

            case OrderProcessingValue.EventOneofCase.DeliveryFinished:
                return HandleDeliveryFinishedAsync(orderId, message.Value.DeliveryFinished, cancellationToken);

            default:
                _logger.LogWarning("Unknown event type for order {OrderId}", orderId);
                return Task.CompletedTask;
        }
    }

    private Task HandleApprovalReceivedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderApprovalReceived approval,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} approval received at {ApprovedAt}",
            orderId,
            approval.CreatedAt.ToDateTime());

        if (!approval.IsApproved)
        {
            _logger.LogInformation("Order {OrderId} approval failed, cancelling order", orderId);
            return _orderService.CancelDuringProcessingAsync(orderId, cancellationToken);
        }

        return WriteLifecycleHistoryAsync(
            orderId,
            "approval_received",
            approval.CreatedAt.ToDateTime(),
            cancellationToken);
    }

    private Task HandlePackingStartedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderPackingStarted packingStarted,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} packing started at {StartedAt}",
            orderId,
            packingStarted.StartedAt.ToDateTime());

        return WriteLifecycleHistoryAsync(
            orderId,
            "packing_started",
            packingStarted.StartedAt.ToDateTime(),
            cancellationToken);
    }

    private Task HandlePackingFinishedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderPackingFinished packingFinished,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} packing finished at {FinishedAt}",
            orderId,
            packingFinished.FinishedAt.ToDateTime());

        if (!packingFinished.IsFinishedSuccessfully)
        {
            _logger.LogInformation("Order {OrderId} packing failed, cancelling order", orderId);
            return _orderService.CancelDuringProcessingAsync(orderId, cancellationToken);
        }

        return WriteLifecycleHistoryAsync(
            orderId,
            "packing_finished",
            packingFinished.FinishedAt.ToDateTime(),
            cancellationToken);
    }

    private Task HandleDeliveryStartedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderDeliveryStarted deliveryStarted,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} delivery started at {StartedAt}",
            orderId,
            deliveryStarted.StartedAt.ToDateTime());

        return WriteLifecycleHistoryAsync(
            orderId,
            "delivery_started",
            deliveryStarted.StartedAt.ToDateTime(),
            cancellationToken);
    }

    private async Task HandleDeliveryFinishedAsync(
        long orderId,
        OrderProcessingValue.Types.OrderDeliveryFinished deliveryFinished,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} delivery finished at {FinishedAt}, completing order",
            orderId,
            deliveryFinished.FinishedAt.ToDateTime());

        if (!deliveryFinished.IsFinishedSuccessfully)
        {
            _logger.LogInformation("Order {OrderId} delivery failed, cancelling order", orderId);
            await _orderService.CancelDuringProcessingAsync(orderId, cancellationToken);
            return;
        }

        await _orderService.CompleteOrderAsync(orderId, cancellationToken);

        _logger.LogInformation("Order {OrderId} completed successfully", orderId);
    }

    private async Task WriteLifecycleHistoryAsync(
        long orderId,
        string toState,
        DateTime atUtc,
        CancellationToken cancellationToken)
    {
        OrderStateChangedPayLoad payload = new()
        {
            FromState = "processing",
            ToState = toState,
        };

        OrderHistoryItem history = _historyFactory.Create(
            orderId,
            OrderHistoryItemKind.StateChanged,
            payload,
            atUtc);

        await _orderHistoryRepository.AddAsync(history, cancellationToken);
    }
}
