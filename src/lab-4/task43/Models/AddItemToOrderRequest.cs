namespace Task43.Models;

public record AddItemToOrderRequest(
    long OrderId,
    long ProductId,
    int Quantity);
