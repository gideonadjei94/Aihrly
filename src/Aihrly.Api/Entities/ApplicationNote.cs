using Aihrly.Api.Enums;

namespace Aihrly.Api.Entities;

public class ApplicationNote
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public NoteType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    // Who wrote this note — resolved from X-Team-Member-Id header, not client input
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Application Application { get; set; } = null!;
    public TeamMember Author { get; set; } = null!;
}
