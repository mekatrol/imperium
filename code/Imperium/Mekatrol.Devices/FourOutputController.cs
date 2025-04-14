using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Imperium.Models;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class FourOutputController(HttpClient client, IPointState pointState, ILogger<FourOutputController> logger) : BaseOutputController(), IFourOutputController
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

        if (deviceInstance.Data is not ControllerConfiguration config)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(ControllerConfiguration).FullName}'.");
        }

        var response = await client.GetAsync($"{config.Url}/outputs", stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: throw a proper error
            throw new Exception($"Failed to read from URL: '{deviceInstance.Key}'");
        }

        // Get the body JSON as a ApiResponse object
        var model = await response.Content.ReadFromJsonAsync<FourOutputControllerModel>(_jsonOptions, stoppingToken);

        // Only the button is an input
        var point = deviceInstance.GetPointWithDefault<int>(nameof(FourOutputControllerModel.Btn));
        pointState.UpdatePointValue(deviceInstance, point, model!.Btn);
    }

    public async Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Writing device instance '{deviceInstance.Key}' using device controller '{this}'");

        if (deviceInstance.Data == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        if (deviceInstance.Data is not ControllerConfiguration config)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(ControllerConfiguration).FullName}'.");
        }

        var model = new FourOutputControllerModel
        {
            Relay1 = GetIntValue(nameof(FourOutputControllerModel.Relay1), deviceInstance, 0),
            Relay2 = GetIntValue(nameof(FourOutputControllerModel.Relay2), deviceInstance, 0),
            Relay3 = GetIntValue(nameof(FourOutputControllerModel.Relay3), deviceInstance, 0),
            Relay4 = GetIntValue(nameof(FourOutputControllerModel.Relay4), deviceInstance, 0),
            Led = GetIntValue(nameof(FourOutputControllerModel.Led), deviceInstance, 0)
        };

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
