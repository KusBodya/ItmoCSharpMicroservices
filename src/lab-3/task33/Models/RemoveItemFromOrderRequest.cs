namespace Task33.Models;

public record RemoveItemFromOrderRequest(
    long OrderId,
    long OrderItemId);
