namespace Aihrly.Api.DTOs.Responses;

public record ApplicationSummaryResponse(
    Guid Id,
    Guid JobId,
    string CandidateName,
    string CandidateEmail,
    string Stage,
    DateTime AppliedAt
);
