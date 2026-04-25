namespace Aihrly.Api.DTOs.Responses;

public record ApplicationProfileResponse(
    Guid Id,
    Guid JobId,
    string JobTitle,
    string CandidateName,
    string CandidateEmail,
    string? CoverLetter,
    string Stage,
    DateTime AppliedAt,
    IReadOnlyList<ScoreResponse> Scores,
    IReadOnlyList<NoteResponse> Notes,
    IReadOnlyList<StageHistoryResponse> StageHistory
);
