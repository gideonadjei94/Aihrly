namespace Aihrly.Api.DTOs.Requests;

public record CreateJobRequest(
    string Title,
    string Description,
    string Location
);
