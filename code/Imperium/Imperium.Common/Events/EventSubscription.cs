using Imperium.Common.Models;

namespace Imperium.Common.Events;

public class EventSubscription(SubscriptionType subscriptionType, EntityType? entityType, string? entityKey)
{
    public SubscriptionType SubscriptionType => subscriptionType;

    public EntityType? EntityType => entityType;

    public string? EntityKey => entityKey;
}
