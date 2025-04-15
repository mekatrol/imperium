namespace Imperium.Server.Middleware;

public static class ExceptionMiddlewareExtensions
{
    public static IServiceCollection AddExceptionMiddleware(this IServiceCollection services)
    {
        services.AddTransient<ExceptionHandlerMiddleware>();
        return services;
    }

    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
