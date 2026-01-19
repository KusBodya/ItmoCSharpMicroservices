namespace Task43.Models;

public record OrderStateChangedPayloadDto(
    string FromState,
    string ToState) : OrderHistoryPayloadDto;
