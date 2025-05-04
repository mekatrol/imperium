using Imperium.Common.Models;

namespace Imperium.Common.Events;

public class SubscriptionEvent(SubscriptionEventType eventType, SubscriptionEventEntityType entityType, string deviceKey, string pointKey, object? value)
{
    public SubscriptionEventType EventType => eventType;

    public SubscriptionEventEntityType EntityType => entityType;

    public string DeviceKey => deviceKey;

    public string PointKey => pointKey;

    public object? Value => value;
}
