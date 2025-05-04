using Imperium.Common.WebSockets;
using System.Net.WebSockets;

namespace Imperium.Common.Services;

internal class WebSocketClientManagerService : IWebSocketClientManagerService
{
    private readonly List<WebSocketClient> _clients = [];
    private readonly Lock _lock = new();

    public void Add(WebSocketClient client)
    {
        lock (_lock)
        {
            _clients.Add(client);
        }
    }

    public void Remove(WebSocketClient client)
    {
        lock (_lock)
        {
            _clients.Remove(client);
        }
    }

    public async Task CloseAll()
    {
        var waitSockets = new List<Task>();
        lock (_lock)
        {
            foreach (var client in _clients)
            {
                if (client.WebSocket.State == WebSocketState.Open)
                {
                    waitSockets.Add(client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, ImperiumConstants.ServerShuttingDownMessage, CancellationToken.None));
                }
            }

            _clients.Clear();
        }

        if (waitSockets.Count > 0)
        {
            await Task.WhenAll(waitSockets);
        }
    }
}
