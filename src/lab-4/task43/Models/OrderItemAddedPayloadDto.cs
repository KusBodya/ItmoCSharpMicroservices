namespace Task43.Models;

public record OrderItemAddedPayloadDto(
    long ProductId,
    int Quantity) : OrderHistoryPayloadDto;
