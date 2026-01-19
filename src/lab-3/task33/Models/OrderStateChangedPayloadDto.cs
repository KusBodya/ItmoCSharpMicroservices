namespace Task33.Models;

public record OrderStateChangedPayloadDto(
    string FromState,
    string ToState) : OrderHistoryPayloadDto;
