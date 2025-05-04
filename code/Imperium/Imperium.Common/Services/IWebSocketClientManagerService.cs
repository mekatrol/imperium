using Imperium.Common.WebSockets;

namespace Imperium.Common.Services;

public interface IWebSocketClientManagerService
{
    void Add(WebSocketClient client);

    void Remove(WebSocketClient client);

    IReadOnlyList<WebSocketClient> GetAll();

    Task CloseAll();
}
