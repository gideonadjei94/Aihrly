namespace Aihrly.Api.DTOs.Responses;

public record NoteResponse(
    Guid Id,
    string Type,
    string Description,
    string AuthorName,
    DateTime CreatedAt
);
