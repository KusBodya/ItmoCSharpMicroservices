namespace Task43.Models;

public record GetOrderHistoryRequest(long OrderId, int PageNumber, int PageSize);
