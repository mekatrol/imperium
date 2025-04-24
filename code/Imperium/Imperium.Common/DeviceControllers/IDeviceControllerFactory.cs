using Imperium.Common.Devices;
using Imperium.Common.Points;

namespace Imperium.Common.DeviceControllers;

public interface IDeviceControllerFactory
{
    IList<string> GetControllerKeys();

    void AddDeviceControllers(IServiceProvider services);

    IDeviceInstance AddDeviceInstance(string deviceKey, string controllerKey, string configurationJson, IList<PointDefinition> points, IImperiumState state);
}
