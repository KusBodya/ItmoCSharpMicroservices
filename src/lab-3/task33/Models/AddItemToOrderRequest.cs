namespace Task33.Models;

public record AddItemToOrderRequest(
    long OrderId,
    long ProductId,
    int Quantity);
