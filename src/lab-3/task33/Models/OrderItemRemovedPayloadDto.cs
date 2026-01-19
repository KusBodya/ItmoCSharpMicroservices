namespace Task33.Models;

public record OrderItemRemovedPayloadDto(
    long ProductId) : OrderHistoryPayloadDto;
