namespace Imperium.Common.Status;

internal class StatusService : IStatusService
{
    private static readonly IList<StatusItemSeverity> _allSeverities;
    private readonly Lock _sync = new();
    private List<IStatusItem> _statuses = [];

    static StatusService()
    {
        _allSeverities = Enum.GetValues<StatusItemSeverity>();
    }

    public Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null)
    {
        return ReportItem(category.ToString(), severity, key, message, correlationId);
    }

    public Guid ReportItem(KnownStatusCategories category, StatusItemSeverity severity, string key, Exception ex, Guid? correlationId = null)
    {
        return ReportItem(category.ToString(), severity, key, ex.ToString(), correlationId);
    }

    public Guid ReportItem(string category, StatusItemSeverity severity, string key, Exception ex, Guid? correlationId = null)
    {
        return ReportItem(category.ToString(), severity, key, ex.ToString(), correlationId);
    }

    public Guid ReportItem(string category, StatusItemSeverity severity, string key, string message, Guid? correlationId = null)
    {
        var statusItem = new StatusItem
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Severity = severity,
            DateTime = DateTime.UtcNow,
            Category = category,
            Key = key,
            Message = message
        };

        lock (_sync)
        {
            _statuses.Add(statusItem);
        }

        return statusItem.CorrelationId;
    }

    public IList<IStatusItem> GetStatuses(IList<StatusItemSeverity>? filter = null)
    {
        filter ??= _allSeverities;

        lock (_sync)
        {
            return _statuses
                .Where(si => filter.Contains(si.Severity))
                .OrderByDescending(si => si.DateTime)
                .ToList();
        }
    }

    public IList<IStatusItem> GetStatuses(TimeSpan range, IList<StatusItemSeverity>? filter = null)
    {
        filter ??= _allSeverities;

        lock (_sync)
        {
            return _statuses
                .Where(si => si.DateTime >= DateTime.UtcNow - range && filter.Contains(si.Severity))
                .ToList();
        }
    }

    public IList<IStatusItem> GetStatuses(DateTime rangeStart, DateTime rangeEnd, IList<StatusItemSeverity>? filter = null)
    {
        filter ??= _allSeverities;

        lock (_sync)
        {
            return _statuses
                .Where(si => si.DateTime >= rangeStart && si.DateTime <= rangeEnd && filter.Contains(si.Severity))
                .ToList();
        }
    }

    public IList<IStatusItem> GetCorrelationStatuses(Guid correlationId)
    {
        lock (_sync)
        {
            return _statuses
                .Where(si => si.CorrelationId == correlationId)
                .ToList();
        }
    }

    public void ClearOlderThan(DateTime? olderThan)
    {
        if (olderThan == null)
        {
            // If older than is null then we clear all so just set a date time in the future 
            // as all statuses will be older than future time
            olderThan = DateTime.UtcNow + TimeSpan.FromDays(1);
        }

        lock (_sync)
        {
            // Get list of statuses we want to keep
            var keepStatusItems = _statuses
                .Where(si => si.DateTime > olderThan)
                .ToList();

            // Replace list with ones we want kept
            _statuses = keepStatusItems;
        }
    }

    public IStatusReporter CreateStatusReporter(KnownStatusCategories category, string key, Guid? correlationId = null)
    {
        return new StatusReporter(this, category, key, correlationId ?? Guid.NewGuid());
    }

    public IStatusReporter CreateStatusReporter(string category, string key, Guid? correlationId = null)
    {
        return new StatusReporter(this, category, key, correlationId ?? Guid.NewGuid());
    }
}
