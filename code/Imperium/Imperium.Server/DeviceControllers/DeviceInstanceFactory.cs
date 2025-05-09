﻿using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Models;
using Imperium.Common.Points;
using System.Reflection;

namespace Imperium.Server.DeviceControllers;

public class DeviceInstanceFactory() : IDeviceInstanceFactory
{
    public IDeviceInstance AddDeviceInstance(
        string deviceKey,
        string controllerKey,
        DeviceType deviceType,
        string? dataJson,
        IList<PointDefinition> points,
        IImperiumState state,
        Assembly? scriptAssembly)
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
           deviceType,
           controllerKey,
           data,
           scriptAssembly: scriptAssembly);

        foreach (var point in points)
        {
            var nativePointType = point.PointType.GetPointNativeType();

            if (nativePointType == null)
            {
                throw new Exception($"The point with key '{point.Key}' has type '{point.PointType}' defined which is not defined.");
            }

            deviceInstance.MapPoint(point.Key, point.FriendlyName, point.Alias, nativePointType, null);
        }

        state.AddDeviceAndPoints(deviceInstance);

        return deviceInstance;
    }
}
