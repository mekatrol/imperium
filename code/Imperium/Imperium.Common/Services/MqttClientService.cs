using Imperium.Common.Configuration;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace Imperium.Common.Services;

internal class MqttClientService(IServiceProvider services, ILogger<MqttClientService> logger) : IMqttClientService
{
    private readonly MqttClientFactory _mqttFactory = new();

    private int _lastMqttHostConfigurationVersion = int.MinValue;
    private IMqttClient? _mqttClient = null;
    private DateTime? _nextConectTryTime = null;

    public async Task Tick(Func<MqttApplicationMessageReceivedEventArgs, Task> applicationMessageReceivedAsync, CancellationToken stoppingToken)
    {
        var statusService = services.GetRequiredService<IStatusService>();
        var statusReporter = statusService.CreateStatusReporter(KnownStatusCategories.Mqtt, nameof(MqttClientService));

        try
        {
            var imperiumDirectories = services.GetRequiredService<ImperiumDirectories>();
            var mqttHostConfiguration = services.GetRequiredService<MqttHostConfiguration>();
            var mqttConfig = mqttHostConfiguration.MqttConfiguration;

            try
            {
                // Has the host configuration changed?
                if (_lastMqttHostConfigurationVersion != mqttHostConfiguration.ChangeVersion ||
                    (_nextConectTryTime != null && DateTime.Now > _nextConectTryTime))
                {
                    // Update to current version
                    _lastMqttHostConfigurationVersion = mqttHostConfiguration.ChangeVersion;

                    // Safely disconnect if any existing connection
                    await SafeDisconnectClient(stoppingToken);

                    // Update configuration
                    mqttConfig = mqttHostConfiguration.MqttConfiguration;

                    // Connect if MQTT host defined
                    if (!await SafeConnectClient(mqttConfig, statusReporter, applicationMessageReceivedAsync, stoppingToken))
                    {
                        // Try and connect again in one minute
                        _nextConectTryTime = DateTime.Now + TimeSpan.FromMinutes(1);
                    }
                    else
                    {
                        // We are connected no need to try to reconnect
                        _nextConectTryTime = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Safely disconnect any existing connection
                await SafeDisconnectClient(stoppingToken);

                // Log status issue
                statusReporter.ReportItem(StatusItemSeverity.Error, ex);

                // Try to connect again in ten seconds if we disconnected
                _nextConectTryTime = DateTime.Now + TimeSpan.FromSeconds(10);
            }
        }
        catch (Exception ex)
        {
            // Safely disconnect any existing connection
            await SafeDisconnectClient(stoppingToken);

            logger.LogError(ex);
            statusReporter.ReportItem(StatusItemSeverity.Error, ex);
        }
    }

    private async Task<bool> SafeConnectClient(
        MqttConfiguration? mqttConfig,
        IStatusReporter statusReporter,
        Func<MqttApplicationMessageReceivedEventArgs, Task> applicationMessageReceivedAsync,
        CancellationToken stoppingToken)
    {
        // Make sure any existing connection terminated
        await SafeDisconnectClient(stoppingToken);

        // Use the first host that matches the key
        var mqttHost = mqttConfig!.Hosts.FirstOrDefault(h => h.Key == ImperiumConstants.MqttKey);

        if (mqttHost == null)
        {
            // No hosts defined
            return true;
        }

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttHost.Server, mqttHost.Port)
            .WithCredentials(mqttHost.User, mqttHost.Password)
            .Build();

        _mqttClient = _mqttFactory.CreateMqttClient();

        _mqttClient.DisconnectedAsync += async (e) =>
        {
            if (e.ClientWasConnected)
            {
                await SafeDisconnectClient(stoppingToken);

                if (e.Exception != null)
                {
                    statusReporter.ReportItem(StatusItemSeverity.Error, e.Exception);
                }
                else if (!string.IsNullOrWhiteSpace(e.ReasonString))
                {
                    statusReporter.ReportItem(StatusItemSeverity.Error, e.ReasonString);
                }
                else
                {
                    statusReporter.ReportItem(StatusItemSeverity.Error, e.Reason.ToString());
                }

                // Try and connect again in one minute
                _nextConectTryTime = DateTime.Now + TimeSpan.FromMinutes(1);
            }
        };

        // Listen for topic message events
        _mqttClient.ApplicationMessageReceivedAsync += applicationMessageReceivedAsync;

        statusReporter.ReportItem(StatusItemSeverity.Information, $"Connecting to host '{mqttHost.Server}'.");

        try
        {
            // Try and connect
            var result = await _mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);

            // Was ther a result
            if (result != null)
            {
                statusReporter.ReportItem(StatusItemSeverity.Information, $"Connect to host '{mqttHost.Server}' result was '{result.ResultCode}'.");

                if (result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    await SafeDisconnectClient(stoppingToken);
                    return false;
                }
            }
            else
            {
                // No result so failed to connect, report and return
                statusReporter.ReportItem(StatusItemSeverity.Error, $"Failed to connect to host '{mqttHost.Server}'.");
                await SafeDisconnectClient(stoppingToken);
                return false;
            }

            // Subscribe to all topics
            var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    topicFilterBuilder =>
                    {
                        // Listen for all topics
                        topicFilterBuilder.WithTopic("#");
                    })
                .Build();

            var response = await _mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);

            // Was ther a result
            if (response != null)
            {
                var firstResult = response.Items.First();

                statusReporter.ReportItem(StatusItemSeverity.Information, $"Connect to host '{mqttHost.Server}' result was '{firstResult.ResultCode}'.");

                if (firstResult.ResultCode > MqttClientSubscribeResultCode.GrantedQoS2)
                {
                    await SafeDisconnectClient(stoppingToken);
                    return false;
                }
            }
            else
            {
                // No result so failed to connect, report and return
                statusReporter.ReportItem(StatusItemSeverity.Error, $"Failed to subscribe to host '{mqttHost.Server}' with topic filter '#'.");
                await SafeDisconnectClient(stoppingToken);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
            statusReporter.ReportItem(StatusItemSeverity.Error, ex);
            return false;
        }
    }

    private async Task SafeDisconnectClient(CancellationToken stoppingToken)
    {
        try
        {
            if (_mqttClient != null)
            {
                try
                {
                    await _mqttClient.DisconnectAsync(cancellationToken: stoppingToken);
                }
                finally
                {
                    _mqttClient.Dispose();
                }
            }
        }
        finally
        {
            _mqttClient = null;
        }
    }
}
