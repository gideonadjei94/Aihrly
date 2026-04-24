using Aihrly.Api.Enums;

namespace Aihrly.Api.Entities;

public class ApplicationScore
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    // Which of the three scoring dimensions this row represents
    public ScoreDimension Dimension { get; set; }

    public int Score { get; set; }
    public string? Comment { get; set; }

    // Tracks the last person who set this score and when
    public Guid ScoredBy { get; set; }
    public DateTime ScoredAt { get; set; }

    public Application Application { get; set; } = null!;
    public TeamMember Scorer { get; set; } = null!;
}
