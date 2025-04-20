using Imperium.Common.Controllers;
using Imperium.Common.Devices;

namespace Imperium.Common.Points;

public interface IImperiumState
{
    string MqttServer { get; set; }

    string MqttUser { get; set; }

    string MqttPassword { get; set; }

    void AddDeviceController(string key, IDeviceController deviceController);

    IDeviceController? GetDeviceController(string controllerKey);

    void AddDeviceAndPoints(IDeviceInstance deviceInstance);
}
