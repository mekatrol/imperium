namespace Imperium.Common.Status;

public class StatusItem : IStatusItem
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    public StatusItemSeverity Severity { get; set; } = StatusItemSeverity.Error;

    public DateTime DateTime { get; set; }

    public string Category { get; set; }

    public string Key { get; set; }

    public string Message { get; set; }
}
