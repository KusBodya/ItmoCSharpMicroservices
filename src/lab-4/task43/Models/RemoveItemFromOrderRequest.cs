namespace Task43.Models;

public record RemoveItemFromOrderRequest(
    long OrderId,
    long OrderItemId);
