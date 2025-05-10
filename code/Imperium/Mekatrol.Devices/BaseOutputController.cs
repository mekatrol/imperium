using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using System.Text.Json;

namespace Mekatrol.Devices;

internal abstract class BaseOutputController : IDeviceController
{
    public object? GetInstanceDataFromJson(string json)
    {
        return JsonSerializer.Deserialize<InstanceConfiguration>(json, JsonSerializerExtensions.DefaultSerializerOptions);
    }

    public abstract Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken);

    public abstract Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken);

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
