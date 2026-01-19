using Application.Ports.Filters;
using Application.Ports.Repo;
using Domain;
using Domain.Enums;
using Domain.PayLoads;
using System.Transactions;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly OrderHistoryFactory _historyFactory;
    private readonly OrderStateValidator _stateValidator;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IOrderHistoryRepository orderHistoryRepository,
        OrderHistoryFactory historyFactory,
        OrderStateValidator stateValidator)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _historyFactory = historyFactory;
        _stateValidator = stateValidator;
    }

    public async Task<Order> CreateOrderAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Order author must be provided.", nameof(createdBy));

        string normalizedAuthor = createdBy.Trim();
        DateTime now = DateTime.UtcNow;

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var order = new Order
        {
            OrderState = OrderState.Created,
            OrderCreatedAt = now,
            OrderCreatedBy = normalizedAuthor,
        };

        order = await _orderRepository.CreateAsync(order, cancellationToken);

        OrderHistoryItem history = _historyFactory.Create(
            order.OrderId,
            OrderHistoryItemKind.Created,
            new OrderCreatedPayLoad { CreatedBy = order.OrderCreatedBy },
            now);

        await _orderHistoryRepository.AddAsync(history, cancellationToken);

        scope.Complete();
        return order;
    }

    public async Task<OrderItem> AddItemAsync(
        long orderId,
        long productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        Order order = await GetRequiredOrderAsync(orderId, cancellationToken);
        _stateValidator.EnsureOrderState(order, new[] { OrderState.Created }, "add items");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var item = new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            OrderItemQuantity = quantity,
            OrderItemDeleted = false,
        };

        item = await _orderItemRepository.AddAsync(item, cancellationToken);

        OrderHistoryItem history = _historyFactory.Create(
            orderId,
            OrderHistoryItemKind.ItemAdded,
            new OrderItemAddedPayLoad
            {
                ProductId = productId,
                Quantity = quantity,
            },
            DateTime.UtcNow);

        await _orderHistoryRepository.AddAsync(history, cancellationToken);

        scope.Complete();
        return item;
    }

    public async Task RemoveItemAsync(long orderId, long orderItemId, CancellationToken cancellationToken = default)
    {
        Order order = await GetRequiredOrderAsync(orderId, cancellationToken);
        _stateValidator.EnsureOrderState(order, new[] { OrderState.Created }, "remove items");

        OrderItem? orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, cancellationToken);
        if (orderItem is null || orderItem.OrderId != orderId)
            throw new InvalidOperationException($"Order item {orderItemId} was not found in order {orderId}.");

        if (orderItem.OrderItemDeleted)
            throw new InvalidOperationException($"Order item {orderItemId} has already been removed.");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _orderItemRepository.SoftDeleteAsync(orderItemId, cancellationToken);

        OrderHistoryItem history = _historyFactory.Create(
            orderId,
            OrderHistoryItemKind.ItemRemoved,
            new OrderItemRemovedPayLoad { ProductId = orderItem.ProductId },
            DateTime.UtcNow);

        await _orderHistoryRepository.AddAsync(history, cancellationToken);

        scope.Complete();
    }

    public Task MoveToProcessingAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return ChangeOrderStateAsync(orderId, OrderState.Processing, new[] { OrderState.Created }, cancellationToken);
    }

    public Task CompleteOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return ChangeOrderStateAsync(orderId, OrderState.Completed, new[] { OrderState.Processing }, cancellationToken);
    }

    public Task CancelOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return ChangeOrderStateAsync(
            orderId,
            OrderState.Cancelled,
            new[] { OrderState.Created, OrderState.Processing },
            cancellationToken);
    }

    public async Task<IReadOnlyList<OrderHistoryItem>> GetHistoryAsync(
        long orderId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be positive.");

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be positive.");

        var filter = new OrderHistorySearchFilter
        {
            OrderIds = new[] { orderId },
        };

        return await _orderHistoryRepository.SearchAsync(filter, pageNumber, pageSize, cancellationToken);
    }

    private async Task ChangeOrderStateAsync(
        long orderId,
        OrderState newState,
        IReadOnlyCollection<OrderState> allowedStates,
        CancellationToken cancellationToken)
    {
        Order order = await GetRequiredOrderAsync(orderId, cancellationToken);
        _stateValidator.EnsureOrderState(order, allowedStates, $"change state to {newState.ToString().ToLowerInvariant()}");

        string fromState = _stateValidator.MapOrderState(order.OrderState);
        string toState = _stateValidator.MapOrderState(newState);

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _orderRepository.UpdateStateAsync(orderId, newState, cancellationToken);

        OrderHistoryItem history = _historyFactory.Create(
            orderId,
            OrderHistoryItemKind.StateChanged,
            new OrderStateChangedPayLoad
            {
                FromState = fromState,
                ToState = toState,
            },
            DateTime.UtcNow);

        await _orderHistoryRepository.AddAsync(history, cancellationToken);

        scope.Complete();
    }

    private async Task<Order> GetRequiredOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        return order ?? throw new InvalidOperationException($"Order {orderId} was not found.");
    }
}
