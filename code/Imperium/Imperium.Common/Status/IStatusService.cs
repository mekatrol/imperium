namespace Imperium.Common.Status;

public interface IStatusService
{
    Guid ReportItem(string category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null);

    Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null);

    IList<IStatusItem> GetStatuses(IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(TimeSpan range, IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(DateTime rangeStart, DateTime rangeEnd, IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetCorrelationStatuses(Guid correlationId);

    void ClearOlderThan(DateTime? olderThan);
}
