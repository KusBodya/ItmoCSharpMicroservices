using Domain41;
using Domain41.Enums;

namespace Application41.Services;

public class OrderStateValidator
{
    public void EnsureOrderState(
        Order order,
        IReadOnlyCollection<OrderState> allowedStates,
        string actionDescription)
    {
        if (!allowedStates.Contains(order.OrderState))
        {
            string allowed = string.Join(", ", allowedStates.Select(s => s.ToString().ToLowerInvariant()));
            throw new InvalidOperationException(
                $"Cannot {actionDescription} while order is in the '{MapOrderState(order.OrderState)}' state. Allowed states: {allowed}.");
        }
    }

    public string MapOrderState(OrderState state)
    {
        return state switch
        {
            OrderState.Created => "created",
            OrderState.Processing => "processing",
            OrderState.Completed => "completed",
            OrderState.Cancelled => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
    }
}
