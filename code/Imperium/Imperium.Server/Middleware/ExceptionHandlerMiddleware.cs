using Imperium.Common.Exceptions;
using Imperium.Common.Extensions;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace Imperium.Server.Middleware;

internal class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ServiceException ex)
        {
            logger.LogError(ex);
            await HandleException(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
            await HandleException(context, ex, HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleException(HttpContext context, Exception ex, HttpStatusCode statusCode)
    {
        await HandleException(context, new ServiceException(statusCode, ex.Message));
    }

    private static async Task HandleException(HttpContext context, ServiceException ex)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)ex.StatusCode;

        var json = JsonSerializer.Serialize(ex.Errors, JsonSerializerExtensions.ErrorSerializerOptions);
        await context.Response.WriteAsync(json);
    }
}
