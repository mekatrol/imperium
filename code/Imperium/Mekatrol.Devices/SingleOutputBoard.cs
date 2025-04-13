using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class SingleOutputBoard(HttpClient client, ILogger<SingleOutputBoard> logger) : ISingleOutputBoard
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task Read(IDeviceInstance deviceInstance, IList<Point> points, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Reading device instance '{deviceInstance}' using device controller '{this}'");

        var response = await client.GetAsync($"{deviceInstance.Key}/outputs", stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: throw a proper error
            throw new Exception($"Failed to read from URL: '{deviceInstance.Key}'");
        }

        // Get the body JSON as a ApiResponse object
        var model = await response.Content.ReadFromJsonAsync<SingleOutputMessageModel>(_jsonOptions, stoppingToken);

        var point = deviceInstance.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Relay));
        point.Value = model!.Relay;

        point = deviceInstance.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Led));
        point.Value = model!.Led;

        point = deviceInstance.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Btn));
        point.Value = model!.Btn;
    }

    public async Task Write(IDeviceInstance deviceInstance, IList<Point> points, CancellationToken stoppingToken)
    {
        logger.LogDebug("{msg}", $"Writing device instance '{deviceInstance}' using device controller '{this}'");

        var pointValues = points.Cast<PointValue<int>>().ToList();

        var model = new SingleOutputMessageModel
        {
            Relay = pointValues.SingleOrDefault(x => x.Key == nameof(SingleOutputMessageModel.Relay))?.Value ?? 0,
            Led = pointValues.SingleOrDefault(x => x.Key == nameof(SingleOutputMessageModel.Led))?.Value ?? 0
        };

        var response = await client.PostAsJsonUnchunked($"{deviceInstance.Key}/outputs", model, _jsonOptions, stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update '{deviceInstance.Key}'");
        }
    }

    public override string ToString()
    {
        return nameof(SingleOutputBoard);
    }
}
