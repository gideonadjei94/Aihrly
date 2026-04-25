namespace Aihrly.Api.DTOs.Requests;

public record AddNoteRequest(
    string Type,
    string Description
);
