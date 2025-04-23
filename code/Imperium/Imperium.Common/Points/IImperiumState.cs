using Imperium.Common.Controllers;
using Imperium.Common.Devices;

namespace Imperium.Common.Points;

public interface IImperiumState
{
    bool IsReadOnlyMode { get; }

    string MqttServer { get; set; }

    string MqttUser { get; set; }

    string MqttPassword { get; set; }

    void AddDeviceController(string key, IDeviceController deviceController);

    IDeviceInstance? GetDeviceInstance(string key, bool includePoints);

    IDeviceController? GetDeviceController(string controllerKey);

    void AddDeviceAndPoints(IDeviceInstance deviceInstance);
}
