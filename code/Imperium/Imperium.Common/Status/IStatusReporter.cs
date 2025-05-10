namespace Imperium.Common.Status;

public interface IStatusReporter
{
    public Guid CorrelationId { get; }

    Guid ReportItem(StatusItemSeverity severity, string message);

    Guid ReportItem(StatusItemSeverity severity, Exception ex);
}
