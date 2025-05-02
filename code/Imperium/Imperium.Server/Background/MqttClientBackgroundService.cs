using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Services;
using Imperium.Common.Status;
using System.Text.RegularExpressions;

namespace Imperium.Server.Background;

public class MqttClientBackgroundService(
    IServiceProvider services,
    ILogger<MqttClientBackgroundService> logger) : BackgroundService()
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttClientService = services.GetRequiredService<IMqttClientService>();
        var state = services.GetRequiredService<IImperiumState>();
        var pointState = services.GetRequiredService<IPointState>();
        var statusService = services.GetRequiredService<IStatusService>();
        var statusReporter = statusService.CreateStatusReporter(KnownStatusCategories.Mqtt, nameof(MqttClientBackgroundService));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await mqttClientService.Tick(async (e) =>
                {
                    try
                    {
                        // Get all device devices that have the mqtt controller 
                        var mqttDevices = state.GetAllDevices()
                            .Where(d => d.ControllerKey == ImperiumConstants.MqttKey)
                            .ToList();

                        var topic = e.ApplicationMessage.Topic;
                        var payload = e.ApplicationMessage.Payload;

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
                        statusReporter.ReportItem(StatusItemSeverity.Error, ex);
                    }
                }, stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
            statusReporter.ReportItem(StatusItemSeverity.Error, ex);
        }
        finally
        {
            statusReporter.ReportItem(StatusItemSeverity.Information, "Exiting background task");
        }
    }
}
