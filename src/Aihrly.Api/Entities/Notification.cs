namespace Aihrly.Api.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    // "hired" or "rejected" — what triggered this notification
    public string Type { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }

    public Application Application { get; set; } = null!;
}
