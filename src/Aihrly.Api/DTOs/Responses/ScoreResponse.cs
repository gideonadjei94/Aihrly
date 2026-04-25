namespace Aihrly.Api.DTOs.Responses;

public record ScoreResponse(
    string Dimension,
    int Score,
    string? Comment,
    string ScoredBy,
    DateTime ScoredAt
);
