namespace Imperium.Server.Middleware;

public class PrivateNetworkCorsHeaderMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase) &&
            context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
        {
            context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
        }

        await _next(context);
    }
}
