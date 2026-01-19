namespace Domain41.PayLoads;

public class OrderItemRemovedPayLoad : OrderHistoryPayLoad
{
    public long ProductId { get; set; }
}
