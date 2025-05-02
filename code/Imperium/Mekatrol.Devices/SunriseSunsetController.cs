using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mekatrol.Devices;

internal class SunriseSunsetController(
    IHttpClientFactory clientFactory,
    IPointState pointState,
    ILogger<SunriseSunsetController> logger) : BaseOutputController(), IDeviceController
{
    private DateOnly _lastReadApi = DateOnly.MinValue;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    private void UpdateIsDaytime(IDeviceInstance deviceInstance)
    {
        var sunrise = (DateTime?)deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunrise)).Value;
        var sunset = (DateTime?)deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunset)).Value;

        if (sunrise == null || sunset == null)
        {
            return;
        }

        var now = DateTime.Now;
        var isDaytime = now.WithinTimeRange(TimeOnly.FromDateTime(sunrise.Value), TimeOnly.FromDateTime(sunset.Value));

        var point = deviceInstance.GetPointWithDefault<bool>("IsDaytime");
        pointState.UpdatePointValue(deviceInstance, point, isDaytime, PointValueType.Device);

        point = deviceInstance.GetPointWithDefault<bool>("IsNighttime");
        pointState.UpdatePointValue(deviceInstance, point, !isDaytime, PointValueType.Device);

        logger.LogDebug("{msg}", $"Sunrise: '{sunrise:HH:mm:ss}', Sunset: '{sunset:HH:mm:ss}', is daytime: '{isDaytime}', now: '{DateTime.Now}', kind: '{DateTime.Now.Kind}'");
    }

    public override Task Write(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
    {
        // Nothing to do in write method
        return Task.CompletedTask;
    }

    public override string ToString()
    {
        return nameof(SunriseSunsetController);
    }

    public async override Task Read(IDeviceInstance deviceInstance, CancellationToken stoppingToken)
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

        try
        {
            // Only once per day
            if (_lastReadApi != DateOnly.MinValue && DateOnly.FromDateTime(DateTime.Now) <= _lastReadApi)
            {
                return;
            }

            var client = clientFactory.CreateClient(nameof(HttpClient));
            var response = await client.GetAsync(config.Url, stoppingToken);

            if (!response.IsSuccessStatusCode)
            {
                // Do nothing, try next time
                return;
            }

            // The device is online and has been communicated with
            deviceInstance.Online = true;
            deviceInstance.LastCommunication = DateTime.UtcNow;

            // Get the body JSON as a ApiResponse object
            var model = await response.Content.ReadFromJsonAsync<SunriseSunsetModel>(_jsonOptions, stoppingToken);

            var point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunrise));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.Sunrise, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.Sunset));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.Sunset, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.SolarNoon));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.SolarNoon, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<int>(nameof(SunriseSunsetResultsModel.DayLength));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.DayLength, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.CivilTwilightBegin, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.CivilTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.CivilTwilightEnd, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.NauticalTwilightBegin, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.NauticalTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.NauticalTwilightEnd, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightBegin));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.AstronomicalTwilightBegin, PointValueType.Device);

            point = deviceInstance.GetPointWithDefault<DateTime>(nameof(SunriseSunsetResultsModel.AstronomicalTwilightEnd));
            pointState.UpdatePointValue(deviceInstance, point, model!.Results.AstronomicalTwilightEnd, PointValueType.Device);

            // Update last read date
            _lastReadApi = DateOnly.FromDateTime(DateTime.Now);

        }
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
        finally
        {
            // Make sure to update daytime flags
            UpdateIsDaytime(deviceInstance);
        }
    }
}
