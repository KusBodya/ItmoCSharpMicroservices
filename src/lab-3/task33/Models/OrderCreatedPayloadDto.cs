namespace Task33.Models;

public record OrderCreatedPayloadDto(
    string CreatedBy) : OrderHistoryPayloadDto;
