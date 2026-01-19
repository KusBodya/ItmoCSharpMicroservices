namespace Task33.Models;

public record OrderDto(
    long OrderId,
    string State,
    DateTime CreatedAt,
    string CreatedBy);
