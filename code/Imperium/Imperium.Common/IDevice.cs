namespace Imperium.Common;

public interface IDevice
{
    /// <summary>
    /// Read all points and data from the device.
    /// </summary>
    Task Read(string url, PointSet points, CancellationToken stoppingToken);

    /// <summary>
    /// Write all points and data to device.
    /// </summary>
    Task Write(string url, PointSet points, CancellationToken stoppingToken);
}
