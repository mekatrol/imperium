namespace Imperium.Common.Status;

internal class StatusReporter(IStatusService statusService, string category, string key, Guid? correlationId = null) : IStatusReporter
{
    private readonly IStatusService _statusService = statusService;
    private readonly string _category = category;
    private readonly string _key = key;
    private readonly Guid? _correlationId = correlationId ?? Guid.NewGuid();

    public StatusReporter(IStatusService statusService, KnownStatusCategories category, string key, Guid? correlationId = null)
        : this(statusService, category.ToString(), key, correlationId)
    {
    }

    public Guid ReportItem(StatusItemSeverity severity, string message)
    {
        return _statusService.ReportItem(_category, severity, _key, message, _correlationId);
    }

    public Guid ReportItem(StatusItemSeverity severity, Exception ex)
    {
        return _statusService.ReportItem(_category, severity, _key, ex, _correlationId);
    }
}
