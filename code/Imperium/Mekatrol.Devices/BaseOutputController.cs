using Imperium.Common.Devices;

namespace Mekatrol.Devices;

internal abstract class BaseOutputController
{
    protected static int GetIntValue(string pointKey, IDeviceInstance deviceInstance, int defaultValue = 0)
    {
        var point = deviceInstance.Points.SingleOrDefault(x => x.Key == pointKey);

        if (point == null)
        {
            return defaultValue;
        }

        if (point.Value == null)
        {
            return defaultValue;
        }

        if (point.Value.GetType() != typeof(int))
        {
            return defaultValue;
        }

        if (point.OverrideValue != null)
        {
            return (int)point.OverrideValue;
        }

        if (point.ControlValue != null)
        {
            return (int)point.ControlValue;
        }

        return (int)point.Value;
    }
}
