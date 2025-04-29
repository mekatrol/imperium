namespace Imperium.Common.Status;

public interface IStatusReporter
{
    Guid ReportItem(StatusItemSeverity severity, string message);
    Guid ReportItem(StatusItemSeverity severity, Exception ex);
}
