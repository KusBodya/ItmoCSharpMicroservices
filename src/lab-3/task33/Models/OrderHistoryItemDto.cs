namespace Task33.Models;

public record OrderHistoryItemDto(
    long HistoryItemId,
    long OrderId,
    DateTime CreatedAt,
    string Kind,
    OrderHistoryPayloadDto? Payload);
