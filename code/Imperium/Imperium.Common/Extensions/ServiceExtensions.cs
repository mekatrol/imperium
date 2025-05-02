using Imperium.Common.Services;
using Imperium.Common.Status;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddImperiumServices(this IServiceCollection services)
    {
        services.AddSingleton<IPointService, PointService>();
        services.AddSingleton<IStatusService, StatusService>();

        services.AddTransient<IMqttClientService, MqttClientService>();

        return services;
    }
}
