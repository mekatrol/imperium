using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.ScriptCompiler;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Imperium.Server.DeviceControllers;

public class MqttPointDeviceController(IServiceProvider services) : IMqttDeviceController
{
    public object? GetInstanceDataFromJson(string json)
    {
        return JsonSerializer.Deserialize<MqttDataConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions);
    }

    public async Task ProcessPayload(
        IDeviceInstance deviceInstance,
        Match topicMatch, 
        ReadOnlySequence<byte> payload,
        IPointState pointState)
    {
        if (payload.IsSingleSegment)
        {
            var json = Encoding.UTF8.GetString(payload.First.Span);

            // If the device has a script assembly then transform the JSON
            if (deviceInstance.ScriptAssembly != null)
            {
                // Transform the JSON file
                json = await ScriptHelper.ExecuteJsonTransformerFromDeviceJsonScript(
                    services,
                    deviceInstance.ScriptAssembly, 
                    json, 
                    CancellationToken.None);
            }

            var payloadObj = JsonSerializer.Deserialize<JsonElement>(json)!;

            if (payloadObj.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in payloadObj.EnumerateObject())
                {
                    var point = deviceInstance.Points.SingleOrDefault(p => property.Name.Equals(p.Alias, StringComparison.OrdinalIgnoreCase));

                    if (point == null)
                    {
                        continue;
                    }

                    point.SetValue(property.Value.ToString(), PointValueType.Control);
                }
            }
        }
        else
        {
            throw new Exception($"While processing topic regex match '{topicMatch}' for device with key '{deviceInstance.Key}' a non single segment payload was received...");
        }
    }

    public Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do, use a virtual contoller for virtual devices
        return Task.CompletedTask;
    }

    public Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do, use a virtual contoller for virtual devices
        return Task.CompletedTask;
    }
}
