using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;

namespace Imperium.Server.DeviceControllers;

public class VirtualPointDeviceController() : IDeviceController
{
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
