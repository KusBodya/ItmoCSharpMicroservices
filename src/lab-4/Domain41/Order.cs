using Domain41.Enums;

namespace Domain41;

public class Order
{
    public long OrderId { get; set; }

    public OrderState OrderState { get; set; }

    public DateTime OrderCreatedAt { get; set; }

    public string OrderCreatedBy { get; set; } = string.Empty;
}