using Imperium.Common.Exceptions;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Imperium.Common.Services;

internal class PointService(IServiceProvider services, ILogger<PointService> logger) : IPointService
{
    public async Task<Point> UpdatePoint(PointUpdateValueModel pointUpdate)
    {
        var pointState = services.GetRequiredService<IPointState>();

        var point = pointState.GetDevicePoint(pointUpdate.DeviceKey, pointUpdate.PointKey);

        if (point == null)
        {
            throw new NotFoundException($"A point with the device key '{pointUpdate.DeviceKey}' and point key '{pointUpdate.PointKey}' was not found.");
        }

        switch (pointUpdate.PointUpdateAction)
        {
            case PointUpdateAction.Control:
                point.SetValue(pointUpdate.Value, PointValueType.Control);
                break;

            case PointUpdateAction.Override:
                point.SetValue(pointUpdate.Value, PointValueType.Override);
                break;

            case PointUpdateAction.OverrideRelease:
                point.SetValue(null, PointValueType.Override);
                break;

            case PointUpdateAction.Toggle:
                point = ToggleValue(pointUpdate, point);
                break;

            default:
                throw new BadRequestException($"The update action '{pointUpdate.PointUpdateAction}' is not handled by '{nameof(PointService)}.{nameof(UpdatePoint)}'.");
        }

        var state = services.GetRequiredService<IImperiumState>();

        // In read only mode we don't update the actual point
        if (state.IsReadOnlyMode)
        {
            return point;
        }

        if (point.DeviceKey == "virtual" || string.IsNullOrWhiteSpace(point.DeviceKey))
        {
            return point;
        }

        // Force an update on the device
        var deviceInstance = state.GetDeviceInstance(point.DeviceKey, true);

        if (deviceInstance == null)
        {
            // No device instance found, log warning and continue
            logger.LogWarning("{msg}", $"The point with key '{point.Key}' specified the device instance with key '{point.DeviceKey}'. A device instance with that key was not found.");
            return point;
        }

        // Get controller used for this instance
        var deviceController = state.GetDeviceController(deviceInstance.ControllerKey);

        if (deviceController == null)
        {
            // No controller found, log warning and continue
            logger.LogWarning("{msg}", $"The device instance with key '{deviceInstance.Key}' specified the device controller with key '{deviceInstance.ControllerKey}'. A device controller with that key was not found.");
            return point;
        }

        try
        {
            logger.LogDebug("{msg}", $"Reading the device instance with key '{deviceInstance.Key}' and controller with key '{deviceInstance.ControllerKey}'.");

            // Write then read all points for this device instance
            await deviceController.Write(deviceInstance, CancellationToken.None);
            await deviceController.Read(deviceInstance, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex);
        }

        return point;
    }

    private static Point ToggleValue(PointUpdateValueModel pointUpdate, Point point)
    {
        if (point.PointType != PointType.Boolean)
        {
            throw new BadRequestException($"The point with the device key '{pointUpdate.DeviceKey}' and point key '{pointUpdate.PointKey}' is not a boolean type and cannot be toggled.");
        }

        // The toggle rules are:
        // 1. If the value is overridden then toggle override value
        // 2. If the control value is not yet set then use the device value and toggle that.
        // 3. Toggle the control value

        // Perform toggle override value
        if (point.OverrideValue != null)
        {
            // New value is inverted override value
            var newOverrideValue = !((bool)point.OverrideValue);
            point.SetValue(newOverrideValue, PointValueType.Override);
            return point;
        }

        // Perform toggle control value
        var newControlValue = !((bool?)point.ControlValue ?? (bool?)point.DeviceValue ?? false);
        point.SetValue(newControlValue, PointValueType.Control);
        return point;
    }
}
