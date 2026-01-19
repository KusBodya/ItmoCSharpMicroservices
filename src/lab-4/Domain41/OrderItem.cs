namespace Domain41;

public class OrderItem
{
    public long OrderItemId { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int OrderItemQuantity { get; set; }

    public bool OrderItemDeleted { get; set; }
}
