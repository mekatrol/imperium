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
        return JsonSerializer.Deserialize<MqttDataConfiguration>(json, JsonSerializerExtensions.DefaultSerializerOptions);
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
            var originalJson = json;

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
                    var point =
                        // Try alias first
                        deviceInstance.Points.SingleOrDefault(p => property.Name.Equals(p.Alias, StringComparison.OrdinalIgnoreCase)) ??
                        // Else property key
                        deviceInstance.Points.SingleOrDefault(p => property.Name.Equals(p.Key, StringComparison.OrdinalIgnoreCase));

                    if (point == null)
                    {
                        // No mapped point
                        continue;
                    }

                    //var fileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                    //var deviceDirectory = Path.Combine("E:\\imperium", deviceInstance.Key);

                    //if (!Directory.Exists(deviceDirectory))
                    //{
                    //    Directory.CreateDirectory(deviceDirectory);
                    //}

                    //await File.WriteAllTextAsync(Path.Combine(deviceDirectory, $"{point.Key}.{fileName}.json"), originalJson);

                    pointState.UpdatePointValue(deviceInstance, point, property.Value.ToString(), PointValueType.Control);

                    // The device is online and has been communicated with
                    deviceInstance.Online = true;
                    deviceInstance.LastCommunication = DateTime.UtcNow;
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
