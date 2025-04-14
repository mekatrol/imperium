using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mekatrol.Devices;

public class SingleOutputController(HttpClient client, IPointState pointState, ILogger<SingleOutputController> logger) : BaseOutputController(), ISingleOutputController
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
        var model = await response.Content.ReadFromJsonAsync<SingleOutputControllerModel>(_jsonOptions, stoppingToken);

        // Only the button is an input
        var point = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Btn));
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

        var model = new SingleOutputControllerModel
        {
            Relay = GetIntValue(nameof(SingleOutputControllerModel.Relay), deviceInstance, 0),
            Led = GetIntValue(nameof(SingleOutputControllerModel.Led), deviceInstance, 0)
        };

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
