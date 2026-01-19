namespace Task33.Models;

public record OrderItemAddedPayloadDto(
    long ProductId,
    int Quantity) : OrderHistoryPayloadDto;
