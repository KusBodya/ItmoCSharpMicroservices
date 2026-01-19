namespace Task43.Models;

public record OrderHistoryItemDto(
    long HistoryItemId,
    long OrderId,
    DateTime CreatedAt,
    string Kind,
    OrderHistoryPayloadDto? Payload);
