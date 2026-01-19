namespace Domain.PayLoads;

public class OrderItemRemovedPayLoad : OrderHistoryPayLoad
{
    public long ProductId { get; set; }
}
