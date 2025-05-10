using Imperium.Common.Devices;
using Imperium.Common.Models;
using Imperium.Common.Points;
using System.Reflection;

namespace Imperium.Common.DeviceControllers;

public interface IDeviceInstanceFactory
{
    IDeviceInstance AddDeviceInstance(
        string deviceKey,
        string controllerKey,
        DeviceType deviceType,
        string? dataJson,
        IList<PointDefinition> points,
        IImperiumState state,
        Assembly? scriptAssembly);
}
