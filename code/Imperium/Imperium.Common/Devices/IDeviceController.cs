namespace Imperium.Common.Devices;

public interface IDeviceController
{
    /// <summary>
    /// Read all points and data from the device.
    /// </summary>
    Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken);

    /// <summary>
    /// Write all points and data to device.
    /// </summary>
    Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken);
}
