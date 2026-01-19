namespace Task43.Models;

public record OrderItemRemovedPayloadDto(
    long ProductId) : OrderHistoryPayloadDto;
