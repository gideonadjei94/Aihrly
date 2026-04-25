namespace Aihrly.Api.DTOs.Requests;

public record MoveStageRequest(
    string Stage,
    string? Reason
);
