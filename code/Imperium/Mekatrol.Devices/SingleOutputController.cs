using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace Mekatrol.Devices;

internal class SingleOutputController(
    IHttpClientFactory clientFactory,
    IPointState pointState,
    ILogger<SingleOutputController> logger) : BaseOutputController(), ISingleOutputController
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async override Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        try
        {
            logger.LogDebug("{msg}", $"Reading device instance '{deviceInstance.Key}' using device controller '{this}'");

            if (deviceInstance.DataJson == null)
            {
                throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
            }

            var config = JsonSerializer.Deserialize<InstanceConfiguration>(deviceInstance.DataJson);

            if (config == null)
            {
                throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(InstanceConfiguration).FullName}'.");
            }

            var client = clientFactory.CreateClient(nameof(HttpClient));
            var response = await client.GetAsync($"{config.Url}/outputs", stoppingToken);

            if (!response.IsSuccessStatusCode)
            {
                // Clear values
                var offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Btn));
                pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

                offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Relay));
                pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

                // TODO: throw a proper error
                throw new Exception($"Failed to read from URL: '{deviceInstance.Key}'");
            }

            // Get the body JSON as a ApiResponse object
            var model = await response.Content.ReadFromJsonAsync<SingleOutputControllerModel>(_jsonOptions, stoppingToken);

            // Update values
            var point = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Btn));
            pointState.UpdatePointValue(deviceInstance, point, model!.Btn, PointValueType.Device);

            var relay = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Relay));
            pointState.UpdatePointValue(deviceInstance, relay, ConvertIntToBool(model!.Relay), PointValueType.Device);
        }
        catch
        {
            // Clear values
            var offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Btn));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Relay));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            throw;
        }
    }

    public async override Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Writing device instance '{deviceInstance.Key}' using device controller '{this}'");

        if (deviceInstance.DataJson == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        var config = JsonSerializer.Deserialize<InstanceConfiguration>(deviceInstance.DataJson);

        if (config == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(InstanceConfiguration).FullName}'.");
        }

        var model = new SingleOutputControllerModel
        {
            Relay = ConvertPointIntToBool(nameof(SingleOutputControllerModel.Relay), deviceInstance, false),
            Led = ConvertPointIntToBool(nameof(SingleOutputControllerModel.Led), deviceInstance, false)
        };

        var client = clientFactory.CreateClient(nameof(HttpClient));
        var response = await client.PostAsJsonUnchunked($"{config.Url}/outputs", model, _jsonOptions, stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update '{deviceInstance.Key}'");
        }
    }

    public override string ToString()
    {
        return nameof(SingleOutputController);
    }
}
