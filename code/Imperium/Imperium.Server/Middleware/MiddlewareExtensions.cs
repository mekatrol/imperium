using System.Text.RegularExpressions;

namespace Imperium.Server.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseInjectApiBaseUrl(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (context.Request.Path.HasValue && IsSpaIndexPage(context.Request.Path.Value))
            {
                // If this is a SPA path, then return the SPA index file
                context.Response.ContentType = "text/html";

                // Read the index.html file
                var indexHtmlContent = await File.ReadAllTextAsync(Path.Combine(env.WebRootPath, "index.html"));

                // Replace api base URL
                var apiBaseUrl = $"{context.Request.Scheme}://{context.Request.Host.Host}:{context.Request.Host.Port}/api";
                indexHtmlContent = indexHtmlContent.Replace("https://localhost:7138/api", apiBaseUrl);

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
        var apiPathPattern = @"^/($|/index.html$)";
        var regex = new Regex(apiPathPattern, RegexOptions.IgnoreCase);

        // If the path does not start with '/api/' or '/swagger/' then is UI path
        return regex.IsMatch(path);
    }

    public static bool IsSpaPath(string path)
    {
        var apiPathPattern = @"^/(api|swagger)(/|$)";
        var regex = new Regex(apiPathPattern, RegexOptions.IgnoreCase);

        // If the path does not start with '/api/' or '/swagger/' then is UI path
        return !regex.IsMatch(path);
    }
}
