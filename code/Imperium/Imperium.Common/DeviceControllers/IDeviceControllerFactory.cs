using Imperium.Common.Devices;
using Imperium.Common.Points;
using System.Reflection;

namespace Imperium.Common.DeviceControllers;

public interface IDeviceControllerFactory
{
    IDeviceInstance? AddDeviceInstance(
        string deviceKey,
        string controllerKey,
        string? dataJson,
        IList<PointDefinition> points,
        IImperiumState state,
        Assembly? scriptAssembly);
}
