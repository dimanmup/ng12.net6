namespace DAL;

public class AuditEvent
{
    public int Id { get; set; }
    public DateTime UtcDateTime { get; set; }
    public int HttpStatusCode { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Объект, с которым произошло событие.
    /// </summary>
    public string Object { get; set; } = "";

    /// <summary>
    /// Источник события.
    /// </summary>
    public string? Source { get; set; }

    public string? SourceIPAddress { get; set; }

    public string? SourceDevice { get; set; }

    public DateTime LocalDateTime => UtcDateTime.ToLocalTime();
}
