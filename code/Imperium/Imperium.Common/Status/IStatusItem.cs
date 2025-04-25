namespace Imperium.Common.Status;

public interface IStatusItem
{
    Guid CorrelationId { get; }

    StatusItemSeverity Severity { get; }

    DateTime DateTime { get; }

    string Category { get; }

    string Key { get; }

    string Message { get; }
}
