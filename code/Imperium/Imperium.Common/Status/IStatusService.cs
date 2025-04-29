namespace Imperium.Common.Status;

public interface IStatusService
{
    IStatusReporter CreateStatusReporter(KnownStatusCategories category, string key, Guid? correlationId = null);

    IStatusReporter CreateStatusReporter(string category, string key, Guid? correlationId = null);

    Guid ReportItem(string category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null);

    Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null);

    Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, Exception ex, Guid? correlationId = null);

    Guid ReportItem(string category, StatusItemSeverity severity, string key, Exception ex, Guid? correlationId = null);

    IList<IStatusItem> GetStatuses(IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(TimeSpan range, IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(DateTime rangeStart, DateTime rangeEnd, IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetCorrelationStatuses(Guid correlationId);

    void ClearOlderThan(DateTime? olderThan);
}
