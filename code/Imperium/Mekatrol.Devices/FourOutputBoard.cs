using System.Net.Http.Json;
using System.Text.Json;
using Imperium.Common;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class FourOutputBoard(HttpClient client, ILogger<FourOutputBoard> logger) : IFourOutputBoard
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
        await Task.Delay(0, stoppingToken);
    }
}
