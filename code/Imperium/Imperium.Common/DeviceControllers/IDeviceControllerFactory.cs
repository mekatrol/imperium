using Imperium.Common.Devices;
using Imperium.Common.Points;

namespace Imperium.Common.DeviceControllers;

public interface IDeviceControllerFactory
{
    IDeviceInstance? AddDeviceInstance(
        string deviceKey, 
        string controllerKey,
        string? dataJson,
        IList<PointDefinition> points, 
        IImperiumState state);
}
