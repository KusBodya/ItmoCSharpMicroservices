namespace Task43.Models;

public record FinishDeliveryRequest(bool IsSuccessful, string? FailureReason = null);
