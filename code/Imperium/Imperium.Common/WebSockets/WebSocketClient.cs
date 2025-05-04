using Imperium.Common.Events;
using System.Net.WebSockets;

namespace Imperium.Common.WebSockets;

public class WebSocketClient(WebSocket webSocket)
{
    private readonly List<EventSubscription> _subscriptions = [];

    public WebSocket WebSocket => webSocket;

    public List<EventSubscription> Subscriptions => _subscriptions;
}
