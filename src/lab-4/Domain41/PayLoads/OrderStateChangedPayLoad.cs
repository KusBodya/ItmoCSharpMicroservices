namespace Domain41.PayLoads;

public class OrderStateChangedPayLoad : OrderHistoryPayLoad
{
    public string FromState { get; set; } = string.Empty;

    public string ToState { get; set; } = string.Empty;
}
