using Aihrly.Api.Enums;

namespace Aihrly.Api.Entities;

public class Job
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Open;
    public DateTime CreatedAt { get; set; }

    public ICollection<Application> Applications { get; set; } = [];
}
