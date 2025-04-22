using Imperium.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddImperiumServices(this IServiceCollection services)
    {
        services.AddSingleton<IPointService, PointService>();

        return services;
    }
}
