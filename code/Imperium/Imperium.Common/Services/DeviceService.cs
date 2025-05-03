using Imperium.Common.Models;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Services;

internal class DeviceService(IServiceProvider services) : IDeviceService
{
    public Task<IList<Device>> GetAllDevices(CancellationToken cancellationToken = default)
    {
        var state = services.GetRequiredService<IImperiumState>();
        var allDevices = state.GetAllDevices();

        IList<Device> devices = allDevices
            .Select(device => new Device
            {
                ControllerKey = device.ControllerKey,
                Enabled = device.Enabled,
                Key = device.Key,
                LastCommunication = device.LastCommunication,
                Online = device.Online
            })
            .ToList();

        return Task.FromResult(devices);
    }
}
