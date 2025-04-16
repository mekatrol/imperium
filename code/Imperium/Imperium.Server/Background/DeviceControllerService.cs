using Imperium.Common.Extensions;
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
    protected override async Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var state = Services.GetRequiredService<ImperiumState>();
        var deviceInstances = state.GetEnabledDeviceInstances(true);

        foreach (var deviceInstance in deviceInstances)
        {
            // Get controller used for this instance
            var deviceController = state.GetDeviceController(deviceInstance.ControllerKey);

            if (deviceController == null)
            {
                // No controller found, log warning and continue
                Logger.LogWarning("{msg}", $"The device instance with key '{deviceInstance.Key}' specified the device controller with key '{deviceInstance.ControllerKey}'. A device controller with that key was not found.");
                continue;
            }

            try
            {
                Logger.LogDebug("{msg}", $"Reading the device instance with key '{deviceInstance.Key}' and controller with key '{deviceInstance.ControllerKey}'.");
             
                // Read all points for this device instance
                await deviceController.Read(deviceInstance, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex);
            }
        }

        var isReadOnlyMode = state.IsReadOnlyMode;

        foreach (var deviceInstance in deviceInstances)
        {
            // Get controller used for this instance
            var deviceController = state.GetDeviceController(deviceInstance.ControllerKey);

            if (deviceController == null)
            {
                // No controller found, log warning and continue
                Logger.LogWarning("{msg}", $"The device instance with key '{deviceInstance.Key}' specified the device controller with key '{deviceInstance.ControllerKey}'. A device controller with that key was not found.");
                continue;
            }

            if (!isReadOnlyMode)
            {
                try
                {
                    Logger.LogDebug("{msg}", $"Writing the device instance with key '{deviceInstance.Key}' and controller with key '{deviceInstance.ControllerKey}'.");
                    await deviceController.Write(deviceInstance, stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex);
                }
            }
        }

        return true;
    }
}
