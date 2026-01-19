namespace Domain41.PayLoads;

public class OrderItemAddedPayLoad : OrderHistoryPayLoad
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}
