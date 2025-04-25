namespace Imperium.Common.Status;

public interface IStatusService
{
    Guid ReportItem(string category, StatusItemSeverity severity, string key, string message);

    Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, string message);

    IList<IStatusItem> GetStatuses(IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(TimeSpan range, IList<StatusItemSeverity>? filter = null);

    IList<IStatusItem> GetStatuses(DateTime rangeStart, DateTime rangeEnd, IList<StatusItemSeverity>? filter = null);

    IStatusItem? GetStatus(Guid correlationId);

    void ClearOlderThan(DateTime? olderThan);
}
