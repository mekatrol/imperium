using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class FourOutputBoard(HttpClient client, ILogger<FourOutputBoard> logger) : IFourOutputBoard
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
        var model = await response.Content.ReadFromJsonAsync<FourOutputMessageModel>(_jsonOptions, stoppingToken);

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Relay1),
            Value = model!.Relay1
        });

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Relay2),
            Value = model!.Relay2
        });

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Relay3),
            Value = model!.Relay3
        });

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Relay4),
            Value = model!.Relay4
        });

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Led),
            Value = model!.Led
        });

        pointSet.Points.Add(new PointValue<int>(PointType.Integer)
        {
            Id = nameof(model.Btn),
            Value = model!.Btn
        });

        await Task.Delay(0, stoppingToken);
    }

    public async Task Write(string url, PointSet pointSet, CancellationToken stoppingToken)
    {
        var points = pointSet.Points.Cast<PointValue<int>>().ToList();

        var model = new FourOutputMessageModel
        {
            Relay1 = points.SingleOrDefault(x => x.Id == nameof(FourOutputMessageModel.Relay1))?.Value ?? 0,
            Relay2 = points.SingleOrDefault(x => x.Id == nameof(FourOutputMessageModel.Relay2))?.Value ?? 0,
            Relay3 = points.SingleOrDefault(x => x.Id == nameof(FourOutputMessageModel.Relay3))?.Value ?? 0,
            Relay4 = points.SingleOrDefault(x => x.Id == nameof(FourOutputMessageModel.Relay4))?.Value ?? 0,
            Led = points.SingleOrDefault(x => x.Id == nameof(SingleOutputMessageModel.Led))?.Value ?? 0
        };

        var response = await client.PostAsJsonUnchunked($"{url}/outputs", model, _jsonOptions, stoppingToken);

        if(!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update '{url}'");
        }
    }
}
