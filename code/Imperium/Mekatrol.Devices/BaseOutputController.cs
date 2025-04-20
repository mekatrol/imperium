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

        return (int)point.Value;
    }
}
