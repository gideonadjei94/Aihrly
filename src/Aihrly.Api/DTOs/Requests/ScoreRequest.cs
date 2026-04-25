namespace Aihrly.Api.DTOs.Requests;

public record ScoreRequest(
    int Score,
    string? Comment
);
