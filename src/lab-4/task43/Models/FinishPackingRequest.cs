namespace Task43.Models;

public record FinishPackingRequest(bool IsSuccessful, string? FailureReason = null);
