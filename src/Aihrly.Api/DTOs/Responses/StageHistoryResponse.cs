namespace Aihrly.Api.DTOs.Responses;

public record StageHistoryResponse(
    string FromStage,
    string ToStage,
    string ChangedBy,
    DateTime ChangedAt,
    string? Reason
);
