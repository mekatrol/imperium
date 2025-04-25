using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using System.Text.Json;

namespace Imperium.Server.DeviceControllers;

public class MqttPointDeviceController : IDeviceController
{
    public object? GetInstanceDataFromJson(string json)
    {
        return JsonSerializer.Deserialize<MqttDataConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions);
    }

    public Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do, use a virtual contoller for virtual devices
        return Task.CompletedTask;
    }

    public Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do, use a virtual contoller for virtual devices
        return Task.CompletedTask;
    }
}
