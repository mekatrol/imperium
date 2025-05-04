
using Imperium.Common.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Imperium.Server.Middleware;

public class WebSocketMiddleware(ICancellationTokenSourceService cancellationTokenSourceService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            // Create a cancellable context for this connection
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            var cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSourceService.Add(cancellationTokenSource);

            // Run WebSocket handling as background task
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"[Task] Received: {message}");

                    // Send updates every 5 seconds
                    for (var i = 0; i < 5; i++)
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        var payload = JsonSerializer.Serialize(new
                        {
                            type = "event",
                            event_type = "state_changed",
                            data = new
                            {
                                entity_id = "sensor.temperature",
                                new_state = new
                                {
                                    state = $"{20 + i} °C",
                                    last_changed = DateTime.UtcNow
                                }
                            }
                        });

                        var bytes = Encoding.UTF8.GetBytes(payload);
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
                        await Task.Delay(5000, cancellationToken);
                    }
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Task] Connection canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Task] WebSocket error: {ex.Message}");
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
