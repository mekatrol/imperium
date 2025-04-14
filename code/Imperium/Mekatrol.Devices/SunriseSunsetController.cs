using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Imperium.Common.Utils;
using Imperium.Models;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class SunriseSunsetController(HttpClient client, IPointState pointState, ILogger<SunriseSunsetController> logger) : BaseOutputController(), IDeviceController
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
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

        var response = await client.GetAsync(config.Url, stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            // Do nothing, try next time
            return;
        }

        try
        {
            // Get the body JSON as a ApiResponse object
            var model = await response.Content.ReadFromJsonAsync<SunriseSunsetModel>(_jsonOptions, stoppingToken);

            var point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunrise));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.Sunrise);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunset));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.Sunset);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.SolarNoon));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.SolarNoon);

            point = deviceInstance.GetPointWithDefault<int>(nameof(SunriseSunsetResultsModel.DayLength));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.DayLength);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.CivilTwilightBegin);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.CivilTwilightEnd);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.NauticalTwilightBegin);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.NauticalTwilightEnd);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.AstronomicalTwilightBegin);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.AstronomicalTwilightEnd);

            var now = DateTime.Now;
            var isDaytime = now.WithinTimeRange(TimeOnly.FromDateTime(model.Results.Sunrise), TimeOnly.FromDateTime(model.Results.Sunset));

            point = deviceInstance.GetPointWithDefault<bool>("IsDaytime");
            pointState.UpdatePointValue(deviceInstance, point, isDaytime);

            point = deviceInstance.GetPointWithDefault<bool>("IsNighttime");
            pointState.UpdatePointValue(deviceInstance, point, !isDaytime);
}
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
    }

    public Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do in write method
        return Task.CompletedTask;
    }

    public override string ToString()
    {
        return nameof(SunriseSunsetController);
    }
}
