using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

    public async Task Read(string url, PointSet pointSet, CancellationToken stoppingToken)
    {
        var response = await client.GetAsync($"{url}/outputs", stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: throw a proper error
            throw new Exception($"Failed to read from URL: '{url}'");
        }

        // Get the body JSON as a ApiResponse object
        var model = await response.Content.ReadFromJsonAsync<SingleOutputMessageModel>(_jsonOptions, stoppingToken);

        var point = pointSet.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Relay));
        point.Value = model!.Relay;

        point = pointSet.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Led));
        point.Value = model!.Led;

        point = pointSet.GetPointWithDefault<PointValue<int>>(nameof(SingleOutputMessageModel.Btn));
        point.Value = model!.Btn;
    }

    public async Task Write(string url, PointSet pointSet, CancellationToken stoppingToken)
    {
        var points = pointSet.Points.Cast<PointValue<int>>().ToList();

        var model = new SingleOutputMessageModel
        {
            Relay = points.SingleOrDefault(x => x.Id == nameof(SingleOutputMessageModel.Relay))?.Value ?? 0,
            Led = points.SingleOrDefault(x => x.Id == nameof(SingleOutputMessageModel.Led))?.Value ?? 0
        };

        var response = await client.PostAsJsonUnchunked($"{url}/outputs", model, _jsonOptions, stoppingToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update '{url}'");
        }
    }
}
