﻿using Imperium.Common.Points;
using System.Text.RegularExpressions;

namespace Imperium.Server.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseInjectApiBaseUrl(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        var state = app.ApplicationServices.GetRequiredService<IImperiumState>();
        var webSocketUri = state.WebSocketUri;

        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (context.Request.Path.HasValue && IsSpaIndexPage(context.Request.Path.Value))
            {
                // If this is a SPA path, then return the SPA index file                
                context.Response.ContentType = "text/html";

                // Read the index.html file
                var indexHtmlContent = await File.ReadAllTextAsync(Path.Combine(env.WebRootPath, "dashboard", "index.html"));

                // Replace base URLs
                var apiBaseUrl = $"{context.Request.Scheme}://{context.Request.Host.Host}:{context.Request.Host.Port}/api";
                var webSocketBaseUrl = $"ws://{webSocketUri.Host}:{webSocketUri.Port}"; ;
                indexHtmlContent = indexHtmlContent.Replace("http://localhost:5000/api", apiBaseUrl);
                indexHtmlContent = indexHtmlContent.Replace("ws://localhost:5001", webSocketBaseUrl);

                // Send the updated file

                await context.Response.WriteAsync(indexHtmlContent);
                return;
            }

            // Call all handlers after this one.
            await next();
        });

        return app;
    }

    public static bool IsSpaIndexPage(string path)
    {
        var indexPathPattern = @"^\/($|index\.html$|dashboard$|dashboard\/$|dashboard\/index\.html$|admin$|admin\/$|admin\/index\.html$)";
        var regex = new Regex(indexPathPattern, RegexOptions.IgnoreCase);

        // If the path does not start with '/api/' or '/swagger/' then is UI path
        return regex.IsMatch(path);
    }

    public static bool IsSpaPath(string path)
    {
        var apiPathPattern = @"^/(dashboard|admin)(/|$)";
        var regex = new Regex(apiPathPattern, RegexOptions.IgnoreCase);

        // If the path starts with '/dashboard/' or '/admin/' then is UI path
        return regex.IsMatch(path);
    }
}
