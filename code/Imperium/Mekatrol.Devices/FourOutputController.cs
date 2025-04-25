using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mekatrol.Devices;

internal class FourOutputController(IHttpClientFactory clientFactory, IPointState pointState, ILogger<FourOutputController> logger) 
    : BaseOutputController(), IFourOutputController
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async override Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
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
            var offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Btn));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay1));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay2));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay3));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            offlinePoint = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay4));
            pointState.UpdatePointValue(deviceInstance, offlinePoint, null, PointValueType.Device);

            // TODO: throw a proper error
            throw new Exception($"Failed to read from URL: '{deviceInstance.Key}'");
        }

        // Get the body JSON as a ApiResponse object
        var model = await response.Content.ReadFromJsonAsync<FourOutputControllerModel>(_jsonOptions, stoppingToken);

        // Update values
        var point = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Btn));
        pointState.UpdatePointValue(deviceInstance, point, model!.Btn, PointValueType.Device);

        var relay1 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay1));
        pointState.UpdatePointValue(deviceInstance, relay1, ConvertIntToBool(model!.Relay1), PointValueType.Device);

        var relay2 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay2));
        pointState.UpdatePointValue(deviceInstance, relay2, ConvertIntToBool(model!.Relay2), PointValueType.Device);

        var relay3 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay3));
        pointState.UpdatePointValue(deviceInstance, relay3, ConvertIntToBool(model!.Relay3), PointValueType.Device);

        var relay4 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay4));
        pointState.UpdatePointValue(deviceInstance, relay4, ConvertIntToBool(model!.Relay4), PointValueType.Device);
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

        var model = new FourOutputControllerModel
        {
            Relay1 = ConvertPointIntToBool(nameof(FourOutputControllerModel.Relay1), deviceInstance, false),
            Relay2 = ConvertPointIntToBool(nameof(FourOutputControllerModel.Relay2), deviceInstance, false),
            Relay3 = ConvertPointIntToBool(nameof(FourOutputControllerModel.Relay3), deviceInstance, false),
            Relay4 = ConvertPointIntToBool(nameof(FourOutputControllerModel.Relay4), deviceInstance, false),
            Led = ConvertPointIntToBool(nameof(FourOutputControllerModel.Led), deviceInstance, false)
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
        return nameof(FourOutputController);
    }
}
