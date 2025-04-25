using Imperium.Common.Devices;

namespace Imperium.Common.DeviceControllers;

public interface IDeviceController
{
    /// <summary>
    /// Convert the JSON instance data to a configuration object.
    /// </summary>
    /// <param name="json"></param>
    object? GetInstanceDataFromJson(string json);

    /// <summary>
    /// Read all points and data from the device.
    /// </summary>
    Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken);

    /// <summary>
    /// Write all points and data to device.
    /// </summary>
    Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken);
}
