using Imperium.Common.Points;
using Imperium.Common.Status;
using Imperium.Server.Options;
using Imperium.Server.State;

namespace Imperium.Server.Background;

internal class DeviceControllerBackgroundService(
    DeviceControllerBackgroundServiceOptions backgroundServiceOptions,
    IServiceProvider serviceProvider,
    ILogger<DeviceControllerBackgroundService> logger)
    : BaseBackgroundService<DeviceControllerBackgroundService>(
        backgroundServiceOptions,
        serviceProvider,
        logger)
{
    private DateTime _nextForceUpdate = DateTime.MinValue;

    protected override async Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var statusService = services.GetRequiredService<IStatusService>();
        var state = Services.GetRequiredService<ImperiumState>();
        var deviceInstances = state.GetEnabledDeviceInstances(true);

        // Force an update every 5 secornds or so (just to cover cases where an update message was not successful, ie lost packets)
        var forceUpdate = _nextForceUpdate < (DateTime.Now - TimeSpan.FromSeconds(5));

        if (forceUpdate)
        {
            _nextForceUpdate = DateTime.Now;

            var readTasks = new List<Task>();

            // We only read devices every force update period
            foreach (var deviceInstance in deviceInstances)
            {
                // Get controller used for this instance
                var deviceController = state.GetDeviceController(deviceInstance.ControllerKey);

                if (deviceController == null)
                {
                    // No controller found, log warning and continue
                    var warning = $"The device instance with key '{deviceInstance.Key}' specified the device controller with key '{deviceInstance.ControllerKey}'. A device controller with that key was not found.";
                    StatusReporter.ReportItem(StatusItemSeverity.Warning, warning);
                    continue;
                }

                try
                {
                    var debug = $"Reading the device instance with key '{deviceInstance.Key}' and controller with key '{deviceInstance.ControllerKey}'.";
                    StatusReporter.ReportItem(StatusItemSeverity.Debug, debug);

                    // Read all points for this device instance
                    readTasks.Add(deviceController.Read(deviceInstance, stoppingToken));
                }
                catch (Exception ex)
                {
                    StatusReporter.ReportItem(StatusItemSeverity.Error, ex);
                }
            }

            try
            {
                await Task.WhenAll(readTasks);
            }
            catch (Exception ex)
            {
                StatusReporter.ReportItem(StatusItemSeverity.Error, ex);
            }
        }

        var isReadOnlyMode = state.IsReadOnlyMode;

        var writeTasks = new List<Task>();
        foreach (var deviceInstance in deviceInstances)
        {
            // Get controller used for this instance
            var deviceController = state.GetDeviceController(deviceInstance.ControllerKey);

            if (deviceController == null)
            {
                // No controller found, log warning and continue
                var warning = $"The device instance with key '{deviceInstance.Key}' specified the device controller with key '{deviceInstance.ControllerKey}'. A device controller with that key was not found.";
                StatusReporter.ReportItem(StatusItemSeverity.Warning, warning);
                continue;
            }

            if (!isReadOnlyMode)
            {
                try
                {
                    var changedPoints = deviceInstance.Points
                        .Where(p => p.HasChanged)
                        .ToList();

                    if (changedPoints.Count > 0 || forceUpdate)
                    {
                        StatusReporter.ReportItem(StatusItemSeverity.Debug, $"Writing the device instance with key '{deviceInstance.Key}' and controller with key '{deviceInstance.ControllerKey}'.");
                        writeTasks.Add(deviceController.Write(deviceInstance, stoppingToken));
                    }

                    // Clear all changed
                    foreach (var point in changedPoints)
                    {
                        // TODO: this only updates the device instance copy
                        point.HasChanged = false;
                    }
                }
                catch (Exception ex)
                {
                    StatusReporter.ReportItem(StatusItemSeverity.Error, ex);
                }
            }
        }

        try
        {
            await Task.WhenAll(writeTasks);
        }
        catch (Exception ex)
        {
            StatusReporter.ReportItem(StatusItemSeverity.Error, ex);
        }

        // Check all devices to see if they have gone offline
        foreach (var deviceInstance in deviceInstances.Where(d => d.DeviceType != DeviceType.Virtual))
        {
            // Check to see if the device has gone offline
            if (deviceInstance.LastCommunication == null || DateTime.UtcNow > (deviceInstance.LastCommunication + deviceInstance.OfflineStatusDuration))
            {
                if (deviceInstance.Online)
                {
                    // The device was flagged as online and is now going offline so report
                    StatusReporter.ReportItem(StatusItemSeverity.Warning, $"The device '{deviceInstance.Key}' has gone offline. The last communication was '{deviceInstance.LastCommunication}'");
                }

                // The last communication has never been set or no update has been received within the offline status dureation
                deviceInstance.Online = false;
            }
        }

        return true;
    }
}
