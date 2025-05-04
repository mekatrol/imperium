
using Imperium.Common;
using Imperium.Common.Events;
using Imperium.Common.Extensions;
using Imperium.Common.Services;
using Imperium.Common.WebSockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Imperium.Server.Middleware;

public class WebSocketMiddleware(
    ICancellationTokenSourceService cancellationTokenSourceService,
    IWebSocketClientManagerService webSocketManager,
    ILogger<WebSocketMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var client = new WebSocketClient(webSocket);
            webSocketManager.Add(client);

            // Create a cancellable context for this connection
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            var cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSourceService.Add(cancellationTokenSource);

            // Run WebSocket handling as background task
            var buffer = new byte[1024 * 4];

            try
            {
                var socketClosing = false;
                while (!socketClosing && webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            try
                            {
                                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                var message = JsonSerializer.Deserialize<EventSubscription>(json, JsonSerializerExtensions.ApiSerializerOptions);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex);
                            }
                            break;

                        case WebSocketMessageType.Binary:
                            logger.LogWarning("{Message}", $"Message type '{result.MessageType}' is not valid for this middleware.");
                            break;

                        case WebSocketMessageType.Close:
                            socketClosing = true;
                            break;

                        default:
                            logger.LogWarning("{Message}", $"Unknown message type '{result.MessageType}'.");
                            break;
                    }

                    var payload = JsonSerializer.Serialize(new ValueChangeEvent("device.point", 23.5));
                    var bytes = Encoding.UTF8.GetBytes(payload);
                    await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
                }

                webSocketManager.Remove(client);

                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, ImperiumConstants.ServerShuttingDownMessage, cancellationToken);
                }
                catch { /* Ignore any close errors - we are exiting anyhow */ }
            }
            catch (OperationCanceledException)
            {
                /* Ignore cancellation */
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex);
            }
            finally
            {
                webSocket.Dispose();
            }

            return;
        }

        await next(context);
    }
}
