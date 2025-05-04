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
        services.AddSingleton<ICancellationTokenSourceService, CancellationTokenSourceService>();

        services.AddScoped<IDeviceService, DeviceService>();

        services.AddTransient<IMqttClientService, MqttClientService>();

        return services;
    }
}
