using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mekatrol.Devices;

internal class SingleOutputController(IHttpClientFactory clientFactory, IPointState pointState, ILogger<SingleOutputController> logger) : BaseOutputController(), ISingleOutputController
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Reading device instance '{deviceInstance.Key}' using device controller '{this}'");

        if (deviceInstance.Data == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        if (deviceInstance.Data is not InstanceConfiguration config)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(InstanceConfiguration).FullName}'.");
        }

        var client = clientFactory.CreateClient(nameof(HttpClient));
        var response = await client.GetAsync($"{config.Url}/outputs", stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
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

    public async Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Writing device instance '{deviceInstance.Key}' using device controller '{this}'");

        if (deviceInstance.Data == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        if (deviceInstance.Data is not InstanceConfiguration config)
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
