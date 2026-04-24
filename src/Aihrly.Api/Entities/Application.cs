using Aihrly.Api.Enums;

namespace Aihrly.Api.Entities;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }

    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }

    public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;
    public DateTime AppliedAt { get; set; }

    public Job Job { get; set; } = null!;
    public ICollection<ApplicationNote> Notes { get; set; } = [];
    public ICollection<ApplicationScore> Scores { get; set; } = [];
    public ICollection<StageHistory> StageHistory { get; set; } = [];
}
