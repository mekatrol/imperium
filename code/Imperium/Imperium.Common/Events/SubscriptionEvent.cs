using Imperium.Common.Models;

namespace Imperium.Common.Events;

public abstract class SubscriptionEvent(SubscriptionEventType eventType, SubscriptionEventEntityType entityType)
{
    public SubscriptionEventType EventType => eventType;

    public SubscriptionEventEntityType EntityType => entityType;
}
