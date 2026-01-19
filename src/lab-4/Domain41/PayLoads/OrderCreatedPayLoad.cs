namespace Domain41.PayLoads;

public class OrderCreatedPayLoad : OrderHistoryPayLoad
{
    public string CreatedBy { get; set; } = string.Empty;
}
