using Imperium.Common.Devices;
using Imperium.Common.Points;

namespace Imperium.Common.Controllers;

public interface IDeviceControllerFactory
{
    void AddDeviceControllers(IServiceProvider services);

    IDeviceInstance AddDeviceInstance(string deviceKey, string controllerKey, string configurationJson, IList<IPointDefinition> points, IImperiumState state);
}
