namespace Task43.Models;

public record OrderHistoryResponseDto(
    IReadOnlyList<OrderHistoryItemDto> Items);
