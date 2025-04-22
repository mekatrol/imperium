using Imperium.Common.Devices;

namespace Mekatrol.Devices;

internal abstract class BaseOutputController
{
    protected static bool? ConvertIntToBool(int? value)
    {
        return value == null ? null : value != 0;
    }

    protected static int ConvertBoolToInt(bool? value)
    {
        return !value.HasValue || value == false ? 0 : 1;
    }

    protected static int ConvertPointIntToBool(string pointKey, IDeviceInstance deviceInstance, bool defaultValue = false)
    {
        var point = deviceInstance.Points.SingleOrDefault(x => x.Key == pointKey);

        if (point?.Value == null || point.Value.GetType() != typeof(bool))
        {
            return ConvertBoolToInt(defaultValue);
        }

        return ConvertBoolToInt((bool?)point.ControlValue);
    }
}
