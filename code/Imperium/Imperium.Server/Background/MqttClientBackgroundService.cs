
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
            using var mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(state.MqttServer)
                .WithCredentials(state.MqttUser, state.MqttPassword)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += applicationMessageEvent =>
            {
                try
                {
                    var topic = applicationMessageEvent.ApplicationMessage.Topic;

                    var regex = new Regex("ness/status/(\\d)");
                    var match = regex.Match(topic);

                    if (!match.Success)
                    {
                        return Task.CompletedTask;
                    }

                    var zone = int.Parse(match.Groups[1].Value);

                    var content = applicationMessageEvent.ApplicationMessage.Payload;

                    if (content.IsSingleSegment)
                    {
                        var json = Encoding.UTF8.GetString(content.First.Span);

                        var zoneStatus = JsonSerializer.Deserialize<ZoneMessage>(json, JsonSerializerExtensions.ApiSerializerOptions);

                        if (zoneStatus != null)
                        {
                            pointState.UpdatePointValue("housealarm", $"zone{zone}", zoneStatus.Event, PointValueType.Device);
                        }
                    }
                    else
                    {
                        // Multi-segment:
                        var x = Encoding.UTF8.GetString(content.ToArray());
                        logger.DebugDump(x);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex);
                }

                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                topicFilterBuilder =>
                {
                    topicFilterBuilder.WithTopic("ness/status/#");
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
        catch (Exception ex)
        {
            logger.LogError(ex);
        }
    }
}

class ZoneMessage
{
    public string Type { get; set; } = string.Empty;

    public int Area { get; set; }

    public int Zone { get; set; }

    public string Event { get; set; } = string.Empty;
}
