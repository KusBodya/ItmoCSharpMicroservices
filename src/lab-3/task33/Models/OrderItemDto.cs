namespace Task33.Models;

public record OrderItemDto(
    long OrderItemId,
    long OrderId,
    long ProductId,
    int Quantity,
    bool Deleted);
