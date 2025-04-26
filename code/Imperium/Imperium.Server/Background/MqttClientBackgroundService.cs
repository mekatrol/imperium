using Imperium.Common;
using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Status;
using MQTTnet;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Imperium.Server.Background;

public class MqttClientBackgroundService(
    IServiceProvider services,
    ILogger<MqttClientBackgroundService> logger) : BackgroundService()
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var state = services.GetRequiredService<IImperiumState>();
            var pointState = services.GetRequiredService<IPointState>();
            var imperiumDirectories = services.GetRequiredService<ImperiumDirectories>();
            var statusService = services.GetRequiredService<IStatusService>();

            MqttConfiguration? mqttConfig = null;

            // Wait for a readable configuration file
            while (mqttConfig == null && !stoppingToken.IsCancellationRequested)
            {
                var mqttHostConfigurationFile = Path.Combine(imperiumDirectories.Base, "mqtt.json");

                if (File.Exists(mqttHostConfigurationFile))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(mqttHostConfigurationFile, stoppingToken);
                        mqttConfig = JsonSerializer.Deserialize<MqttConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions);
                    }
                    catch (Exception ex)
                    {
                        statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Error, nameof(MqttClientBackgroundService), ex.Message);
                    }
                }

                if (mqttConfig != null && mqttConfig.Hosts.Count > 0)
                {
                    break;
                }

                await Task.Delay(5000, stoppingToken);
            }

            var mqttHost = mqttConfig!.Hosts.First();
            var mqttFactory = new MqttClientFactory();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var mqttClient = mqttFactory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(mqttHost.Server, mqttHost.Port)
                    .WithCredentials(mqttHost.User, mqttHost.Password)
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += async (applicationMessageEvent) =>
                {
                    try
                    {
                        // Get all device devices that have the mqtt controller 
                        var mqttDevices = state.GetAllDevices()
                            .Where(d => d.ControllerKey == ImperiumConstants.MqttKey)
                            .ToList();

                        var topic = applicationMessageEvent.ApplicationMessage.Topic;
                        var payload = applicationMessageEvent.ApplicationMessage.Payload;

                        if (state.GetDeviceController(ImperiumConstants.MqttKey) is IMqttDeviceController controller)
                        {
                            foreach (var deviceInstance in mqttDevices)
                            {
                                var data = (MqttDataConfiguration)deviceInstance.Data!;

                                var topicFilterRegex = new Regex(data.Topic);
                                var topicMatch = topicFilterRegex.Match(topic);

                                if (topicMatch.Success)
                                {
                                    try
                                    {
                                        await controller.ProcessPayload(deviceInstance, topicMatch, payload, pointState);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogWarning(ex);
                                    }
                                }
                            }
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
}
