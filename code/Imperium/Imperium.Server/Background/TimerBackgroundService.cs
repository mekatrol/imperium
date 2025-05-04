using Imperium.Common.Configuration;
using Imperium.Common.Directories;
using Imperium.Common.Events;
using Imperium.Common.Extensions;
using Imperium.Common.Models;
using Imperium.Common.Points;
using Imperium.Common.Services;
using Imperium.Common.Status;
using Imperium.Server.Options;
using Imperium.Server.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Imperium.Server.Background;

internal class TimerBackgroundService(
    TimerBackgroundServiceOptions backgroundServiceOptions,
    IServiceProvider serviceProvider,
    ILogger<TimerBackgroundService> logger)
    : BaseBackgroundService<TimerBackgroundService>(
        backgroundServiceOptions,
        serviceProvider,
        logger)
{
    private DateTime _nextPublishAllDateTime = DateTime.MinValue;
    private DateTime _lastTickDateTime = DateTime.Now;
    private bool _mqttHostConfigurationErrorStatusReported = false;

    protected override async Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var now = DateTime.Now;

        // Are we in the same hour as last tick?
        if (_lastTickDateTime.Hour != now.Hour)
        {
            _lastTickDateTime = now;

            // This will force a reload of the SPA dashboard UI
            var appVersionService = services.GetRequiredService<IAppVersionService>();
            appVersionService.ExecutionVersion = Guid.NewGuid();
        }

        if (_nextPublishAllDateTime < (now - TimeSpan.FromSeconds(10)))
        {

        }

        await UpdateMqttHostConfiguration(services, stoppingToken);
        await ProcessEvents(services, stoppingToken);

        if (_nextPublishAllDateTime < (now - TimeSpan.FromSeconds(10)))
        {
            _nextPublishAllDateTime = now;
            await PublishAll(services, stoppingToken);
        }

        return true;
    }

    private async Task PublishAll(IServiceProvider services, CancellationToken stoppingToken)
    {
        var state = services.GetRequiredService<IImperiumState>();
        var webSocketClientManager = services.GetRequiredService<IWebSocketClientManagerService>();

        var points = state.GetAllPoints();
        var clients = webSocketClientManager.GetAll();

        foreach (var point in points)
        {
            var payload = JsonSerializer.Serialize(
                new SubscriptionEvent(
                    SubscriptionEventType.Refresh,
                    SubscriptionEventEntityType.Point,
                    point.DeviceKey,
                    point.Key,
                    point.Value),
                JsonSerializerExtensions.ApiSerializerOptions);

            var bytes = Encoding.UTF8.GetBytes(payload);

            foreach (var client in clients)
            {
                try
                {
                    await client.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex);
                }
            }
        }
    }

    private async Task ProcessEvents(IServiceProvider services, CancellationToken stoppingToken)
    {
        var state = services.GetRequiredService<IImperiumState>();
        var webSocketClientManager = services.GetRequiredService<IWebSocketClientManagerService>();

        if (state.ChangeEvents.TryDequeue(out var changeEvent))
        {
            var payload = JsonSerializer.Serialize(changeEvent, JsonSerializerExtensions.ApiSerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var clients = webSocketClientManager.GetAll();

            foreach (var client in clients)
            {
                try
                {
                    await client.WebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex);
                }
            }
        }
    }

    private async Task UpdateMqttHostConfiguration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var imperiumDirectories = services.GetRequiredService<ImperiumDirectories>();
        var mqttHostConfigurationFile = Path.Combine(imperiumDirectories.Base, "mqtt.json");
        var statusService = services.GetRequiredService<IStatusService>();
        var mqttHostConfiguration = services.GetRequiredService<MqttHostConfiguration>();

        var mqttConfig = mqttHostConfiguration.MqttConfiguration;

        try
        {
            var json = await File.ReadAllTextAsync(mqttHostConfigurationFile, stoppingToken);
            mqttConfig = JsonSerializer.Deserialize<MqttConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions);

            // Reset have reported configuration error
            _mqttHostConfigurationErrorStatusReported = false;
        }
        catch (Exception ex)
        {
            if (!_mqttHostConfigurationErrorStatusReported)
            {
                statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Error, nameof(MqttClientBackgroundService), ex.Message);

                // Set have reported configuration error
                _mqttHostConfigurationErrorStatusReported = true;
            }

            mqttConfig = null;
        }

        mqttHostConfiguration.MqttConfiguration = mqttConfig;
    }
}
