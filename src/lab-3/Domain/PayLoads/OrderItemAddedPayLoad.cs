namespace Domain.PayLoads;

public class OrderItemAddedPayLoad : OrderHistoryPayLoad
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}
