using Aihrly.Api.Enums;

namespace Aihrly.Api.Entities;

public class StageHistory
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public ApplicationStage FromStage { get; set; }
    public ApplicationStage ToStage { get; set; }

    // Who triggered the move — from X-Team-Member-Id header
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }

    public Application Application { get; set; } = null!;
    public TeamMember ChangedByMember { get; set; } = null!;
}
