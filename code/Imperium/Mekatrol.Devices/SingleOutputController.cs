﻿using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class SingleOutputController(HttpClient client, ILogger<SingleOutputController> logger) : BaseOutputController(), ISingleOutputController
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Reading device instance '{deviceInstance.Key}' using device controller '{this}'");

        if(deviceInstance.Data == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        if(deviceInstance.Data is not OutputControllerConfiguration config)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(OutputControllerConfiguration).FullName}'.");
        }

        var response = await client.GetAsync($"{config.Url}/outputs", stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: throw a proper error
            throw new Exception($"Failed to read from URL: '{deviceInstance.Key}'");
        }

        // Get the body JSON as a ApiResponse object
        var model = await response.Content.ReadFromJsonAsync<SingleOutputControllerModel>(_jsonOptions, stoppingToken);

        var point = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Relay));
        point.Value = model!.Relay;

        point = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Led));
        point.Value = model!.Led;

        point = deviceInstance.GetPointWithDefault<int>(nameof(SingleOutputControllerModel.Btn));
        point.Value = model!.Btn;
    }

    public async Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Writing device instance '{deviceInstance.Key}' using device controller '{this}'");

        if (deviceInstance.Data == null)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' does not have any configuration data set.");
        }

        if (deviceInstance.Data is not OutputControllerConfiguration config)
        {
            throw new InvalidDataException($"Device instance '{deviceInstance.Key}' data is not of type '{typeof(OutputControllerConfiguration).FullName}'.");
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
