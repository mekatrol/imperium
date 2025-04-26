using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;

namespace Imperium.Common.Points;

public interface IImperiumState
{
    bool IsReadOnlyMode { get; }

    IList<Point> GetAllPoints();

    IList<IDeviceInstance> GetAllDevices();

    void AddDeviceController(string key, IDeviceController deviceController);

    IDeviceInstance? GetDeviceInstance(string key, bool includePoints);

    IDictionary<string, IDeviceController> GetDeviceControllers();

    IDeviceController? GetDeviceController(string controllerKey);

    void AddDeviceAndPoints(IDeviceInstance deviceInstance);
}
