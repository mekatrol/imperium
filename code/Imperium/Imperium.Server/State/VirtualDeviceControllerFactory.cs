using Imperium.Common;
using Imperium.Common.Controllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Server.Models;
using System.Text.Json;

namespace Imperium.Server.State;

public class VirtualDeviceControllerFactory : IDeviceControllerFactory
{
    public IList<string> GetControllerKeys()
    {
        return [ImperiumConstants.VirtualKey];
    }

    public void AddDeviceControllers(IServiceProvider services)
    {
        var state = services.GetRequiredService<IImperiumState>();
        state.AddDeviceController(ImperiumConstants.VirtualKey, new VirtualPointDeviceController());
    }

    public IDeviceInstance AddDeviceInstance(string deviceKey, string controllerKey, string configurationJson, IList<PointDefinition> points, IImperiumState state)
    {
        switch (controllerKey.ToLower())
        {
            case ImperiumConstants.VirtualKey:
                return AddDevice<VirtualPointDeviceController>(deviceKey, controllerKey, configurationJson, points, state);

            default:
                throw new ArgumentException($"{nameof(VirtualDeviceControllerFactory)} does not implement a controller with the key '{controllerKey}'.");
        }
    }

    private static IDeviceInstance AddDevice<TModel>(string deviceKey,
        string controllerKey,
        string configurationJson,
        IList<PointDefinition> points,
        IImperiumState state)
    {
        // Get the controller
        var controller = state.GetDeviceController(controllerKey);

        if (controller == null)
        {
            throw new Exception($"A controller with the key '{controllerKey}' has not been added. Please call '{nameof(VirtualDeviceControllerFactory)}.{nameof(AddDeviceControllers)}' before calling this method.");
        }

        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            // Default to empty JSON object
            configurationJson = "{}";
        }

        var instanceConfiguration = JsonSerializer.Deserialize<VirtualDeviceInstanceConfiguration>(configurationJson);

        // get list of valid points based on model
        var pointProperties = typeof(TModel).GetProperties()
            .ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

        var deviceInstance = new DeviceInstance<VirtualDeviceInstanceConfiguration>(
           deviceKey,
           DeviceType.Physical,
           controllerKey,
           instanceConfiguration);

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
