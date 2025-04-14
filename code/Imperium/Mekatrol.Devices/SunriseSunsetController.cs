using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Imperium.Common.Utils;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class SunriseSunsetController(HttpClient client, ILogger<SunriseSunsetController> logger) : BaseOutputController(), IDeviceController
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
            point.Value = model!.Results.Sunrise;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunset));
            point.Value = model!.Results.Sunset;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.SolarNoon));
            point.Value = model!.Results.SolarNoon;

            point = deviceInstance.GetPointWithDefault<int>(nameof(SunriseSunsetResultsModel.DayLength));
            point.Value = model!.Results.DayLength;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightBegin));
            point.Value = model!.Results.CivilTwilightBegin;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightEnd));
            point.Value = model!.Results.CivilTwilightEnd;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightBegin));
            point.Value = model!.Results.NauticalTwilightBegin;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightEnd));
            point.Value = model!.Results.NauticalTwilightEnd;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightBegin));
            point.Value = model!.Results.AstronomicalTwilightBegin;

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightEnd));
            point.Value = model!.Results.AstronomicalTwilightEnd;

            var now = DateTime.Now;
            var isDaytime = now.WithinTimeRange(TimeOnly.FromDateTime(model.Results.Sunrise), TimeOnly.FromDateTime(model.Results.Sunset));

            point = deviceInstance.GetPointWithDefault<bool>("IsDaytime");
            point.Value = isDaytime;
                
            point = deviceInstance.GetPointWithDefault<bool>("IsNighttime");            
            point.Value = !isDaytime;
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
