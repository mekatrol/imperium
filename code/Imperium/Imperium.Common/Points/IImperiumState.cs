using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;

namespace Imperium.Common.Points;

public interface IImperiumState
{
    bool IsReadOnlyMode { get; }

    string MqttServer { get; set; }

    string MqttUser { get; set; }

    string MqttPassword { get; set; }

    IList<Point> GetAllPoints();

    void AddDeviceController(string key, IDeviceController deviceController);

    IDeviceInstance? GetDeviceInstance(string key, bool includePoints);

    IDeviceController? GetDeviceController(string controllerKey);

    void AddDeviceAndPoints(IDeviceInstance deviceInstance);
}
