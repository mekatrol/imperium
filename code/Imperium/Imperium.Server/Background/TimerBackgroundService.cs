using Imperium.Common.Configuration;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Status;
using Imperium.Server.Options;
using Imperium.Server.Services;
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

        await UpdateMqttHostConfiguration(services, stoppingToken);

        return true;
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
