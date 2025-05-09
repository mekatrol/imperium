﻿using Imperium.Common.Services;
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
        services.AddSingleton<IWebSocketClientManagerService, WebSocketClientManagerService>();

        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISwitchService, SwitchService>();

        services.AddTransient<IMqttClientService, MqttClientService>();

        return services;
    }
}
