namespace Aihrly.Api.DTOs.Requests;

public record CreateApplicationRequest(
    string CandidateName,
    string CandidateEmail,
    string? CoverLetter
);
