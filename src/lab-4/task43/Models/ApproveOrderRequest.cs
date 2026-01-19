namespace Task43.Models;

public record ApproveOrderRequest(bool IsApproved, string ApprovedBy, string? FailureReason = null);
