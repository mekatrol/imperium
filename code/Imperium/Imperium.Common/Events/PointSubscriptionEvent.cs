using Imperium.Common.Models;
using Imperium.Common.Points;

namespace Imperium.Common.Events;

public class PointSubscriptionEvent(SubscriptionEventType eventType, SubscriptionEventEntityType entityType, Point point)
    : SubscriptionEvent(eventType, entityType)
{
    public Point Point => point;
}
