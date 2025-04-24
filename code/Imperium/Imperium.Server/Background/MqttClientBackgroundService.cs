
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using MQTTnet;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Imperium.Server.Background;

public class MqttClientBackgroundService(
    IImperiumState state,
    IPointState pointState,
    ILogger<MqttClientBackgroundService> logger) : BackgroundService()
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var mqttFactory = new MqttClientFactory();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var mqttClient = mqttFactory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(state.MqttServer)
                    .WithCredentials(state.MqttUser, state.MqttPassword)
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += async (applicationMessageEvent) =>
                {
                    try
                    {
                        var topic = applicationMessageEvent.ApplicationMessage.Topic;
                        var payload = applicationMessageEvent.ApplicationMessage.Payload;

                        var regex = new Regex("ness/status/(\\d)");
                        var match = regex.Match(topic);

                        if (match.Success)
                        {
                            await ProcessHouseAlarmMessage(match, payload);
                        }

                        regex = new Regex("dog/status");
                        match = regex.Match(topic);

                        if (match.Success)
                        {
                            await ProcessDogHeaterMessage(payload);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex);
                    }
                };

                await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    topicFilterBuilder =>
                    {
                        // Listen for all topics
                        topicFilterBuilder.WithTopic("#");
                    })
                .Build();

                var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

                logger.DebugDump(response);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                await mqttClient
                    .DisconnectAsync(new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
                    .Build(), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
    }

    private Task ProcessHouseAlarmMessage(Match match, ReadOnlySequence<byte> payload)
    {
        var zone = int.Parse(match.Groups[1].Value);

        if (payload.IsSingleSegment)
        {
            var json = Encoding.UTF8.GetString(payload.First.Span);

            var zoneStatus = JsonSerializer.Deserialize<ZoneMessage>(json, JsonSerializerExtensions.ApiSerializerOptions);

            if (zoneStatus != null)
            {
                pointState.UpdatePointValue("housealarm", $"zone{zone}", zoneStatus.Event, PointValueType.Device);
            }
        }

        return Task.CompletedTask;
    }

    private Task ProcessDogHeaterMessage(ReadOnlySequence<byte> payload)
    {
        if (payload.IsSingleSegment)
        {
            var json = Encoding.UTF8.GetString(payload.First.Span);

            var status = JsonSerializer.Deserialize<DogHeaterStatusMessage>(json, JsonSerializerExtensions.ApiSerializerOptions);

            if (status != null)
            {
                pointState.UpdatePointValue("dogheater", "temp.1", status.Temperature1, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "temp.2", status.Temperature2, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "temp.avg", status.TemperatureAverage, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "temp.sp", status.TemperatureSetpoint, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "temp.pb", status.TemperatureProportionalBand, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "heater.on", status.HeaterOn, PointValueType.Device);
                pointState.UpdatePointValue("dogheater", "heater.enabled", status.Enabled, PointValueType.Device);
            }
        }

        return Task.CompletedTask;
    }
}

class ZoneMessage
{
    public string Type { get; set; } = string.Empty;

    public int Area { get; set; }

    public int Zone { get; set; }

    public string Event { get; set; } = string.Empty;
}

class DogHeaterStatusMessage
{
    public float Temperature1 { get; set; }

    public float Temperature2 { get; set; }

    public float TemperatureAverage { get; set; }

    public float TemperatureSetpoint { get; set; }

    public float TemperatureProportionalBand { get; set; }

    public bool HeaterOn { get; set; }

    public bool Enabled { get; set; }
}
