namespace Aihrly.Api.DTOs.Responses;

public record JobResponse(
    Guid Id,
    string Title,
    string Description,
    string Location,
    string Status,
    DateTime CreatedAt
);
