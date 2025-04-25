using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;

namespace Imperium.Server.DeviceControllers;

public class DeviceControllerFactory() : IDeviceControllerFactory
{
    public IDeviceInstance? AddDeviceInstance(
        string deviceKey,
        string controllerKey,
        string? dataJson,
        IList<PointDefinition> points,
        IImperiumState state)
    {
        // Get the controller by get controller key.
        var controller = state.GetDeviceController(controllerKey);

        // If none found then no controller with the key has been registered and so throw exception.
        if (controller == null)
        {
            throw new Exception($"A controller with the key '{controllerKey}' has not been registered.");
        }

        object? data = null;

        if (dataJson != null)
        {
            data = controller.GetInstanceDataFromJson(dataJson);
        }

        var deviceInstance = new DeviceInstance(
           deviceKey,
           DeviceType.Physical,
           controllerKey,
           data);

        foreach (var point in points)
        {
            var nativePointType = point.PointType.GetPointNativeType();

            if (nativePointType == null)
            {
                throw new Exception($"The point with key '{point.Key}' has type '{point.PointType}' defined which is not defined.");
            }

            deviceInstance.MapPoint(point.Key, point.FriendlyName, nativePointType, null);
        }

        state.AddDeviceAndPoints(deviceInstance);

        return deviceInstance;
    }
}
