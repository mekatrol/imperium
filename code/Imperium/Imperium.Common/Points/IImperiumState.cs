using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Events;
using System.Collections.Concurrent;

namespace Imperium.Common.Points;

public interface IImperiumState
{
    bool IsReadOnlyMode { get; }

    Uri WebSocketUri { get; }

    ConcurrentQueue<SubscriptionEvent> ChangeEvents { get; }

    IList<Point> GetAllPoints();

    IList<IDeviceInstance> GetAllDevices();

    void AddDeviceController(string key, IDeviceController deviceController);

    IDeviceInstance? GetDeviceInstance(string key, bool includePoints);

    IDictionary<string, IDeviceController> GetDeviceControllers();

    IDeviceController? GetDeviceController(string controllerKey);

    void AddDeviceAndPoints(IDeviceInstance deviceInstance);
}
