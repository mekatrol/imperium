using Imperium.Common.Exceptions;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Services;

internal class PointService(IServiceProvider services) : IPointService
{
    public Task<Point> UpdatePoint(PointUpdateValueModel pointUpdate)
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
                return Task.FromResult(point);

            case PointUpdateAction.Override:
                point.SetValue(pointUpdate.Value, PointValueType.Override);
                return Task.FromResult(point);

            case PointUpdateAction.OverrideRelease:
                point.SetValue(null, PointValueType.Override);
                return Task.FromResult(point);

            case PointUpdateAction.Toggle:
                return ToggleValue(pointUpdate, point);

            default:
                throw new BadRequestException($"The update action '{pointUpdate.PointUpdateAction}' is not handled by '{nameof(PointService)}.{nameof(UpdatePoint)}'.");
        }
    }

    private static Task<Point> ToggleValue(PointUpdateValueModel pointUpdate, Point point)
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
            return Task.FromResult(point);
        }

        // Perform toggle control value
        var newControlValue = !((bool?)point.ControlValue ?? (bool?)point.DeviceValue ?? false);
        point.SetValue(newControlValue, PointValueType.Control);
        return Task.FromResult(point);
    }
}
