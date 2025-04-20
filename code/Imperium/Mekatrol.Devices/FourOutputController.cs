using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mekatrol.Devices;

internal class FourOutputController(IHttpClientFactory clientFactory, IPointState pointState, ILogger<FourOutputController> logger) : BaseOutputController(), IFourOutputController
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
        var model = await response.Content.ReadFromJsonAsync<FourOutputControllerModel>(_jsonOptions, stoppingToken);

        // Update values
        var point = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Btn));
        pointState.UpdatePointValue(deviceInstance, point, model!.Btn);

        var relay1 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay1));
        pointState.UpdatePointValue(deviceInstance, relay1, model!.Relay1);

        var relay2 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay2));
        pointState.UpdatePointValue(deviceInstance, relay2, model!.Relay2);

        var relay3 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay3));
        pointState.UpdatePointValue(deviceInstance, relay3, model!.Relay3);

        var relay4 = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Relay4));
        pointState.UpdatePointValue(deviceInstance, relay4, model!.Relay4);
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

        var model = new FourOutputControllerModel
        {
            Relay1 = GetIntValue(nameof(FourOutputControllerModel.Relay1), deviceInstance, 0),
            Relay2 = GetIntValue(nameof(FourOutputControllerModel.Relay2), deviceInstance, 0),
            Relay3 = GetIntValue(nameof(FourOutputControllerModel.Relay3), deviceInstance, 0),
            Relay4 = GetIntValue(nameof(FourOutputControllerModel.Relay4), deviceInstance, 0),
            Led = GetIntValue(nameof(FourOutputControllerModel.Led), deviceInstance, 0)
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
