namespace Task33.Models;

public record OrderHistoryResponseDto(
    IReadOnlyList<OrderHistoryItemDto> Items);
