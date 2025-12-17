namespace FortressApi.Models;

public sealed class AuditEvent
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;     // NOTE_CREATED, LOGIN, etc.
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;   // e.g., /api/notes/{id}
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
