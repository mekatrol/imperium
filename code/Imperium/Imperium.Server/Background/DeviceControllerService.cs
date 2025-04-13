using Imperium.Common.Extensions;
using Imperium.Models;

namespace Imperium.Server.Background;

internal class DeviceControllerBackgroundService(
    BackgroundServiceOptions backgroundServiceOptions,
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

            // Read all points for this device instance
            await deviceController.Read(deviceInstance, stoppingToken);
        }

        /*****************************************************************************
         * START FLOW LOGIC
         *****************************************************************************/

        //var now = DateTime.Now;

        //// Alfresco on between 19:30 and 06:45
        //var alfrescoOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(6, 45));

        //// String lights on between 19:30 and 22:30
        //var stringOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(22, 30));

        //// Fish plant pump on between 07:30 and 19:30
        //var fishPlantsOn = now.WithinTimeRange(new TimeOnly(07, 30), new TimeOnly(19, 30));

        //var alfrescoLightPoint = alfrescoLightPoints.Points
        //    .Cast<PointValue<int>>()
        //    .SingleOrDefault(x => x.Key == "Relay");

        //alfrescoLightPoint!.Value = alfrescoOn ? 1 : 0;

        //var stringLightPoint = kitchenViewPoints.Points
        //    .Cast<PointValue<int>>()
        //    .SingleOrDefault(x => x.Key == "Relay1");

        //stringLightPoint!.Value = stringOn ? 1 : 0;

        //var fishPlantPump = carportPoints.Points
        //    .Cast<PointValue<int>>()
        //    .SingleOrDefault(x => x.Key == "Relay4");

        //fishPlantPump!.Value = fishPlantsOn ? 1 : 0;

        /*****************************************************************************
         * END FLOW LOGIC
         *****************************************************************************/

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

            // Get all points for this device instance
            var points = state.GetDevicePoints(deviceInstance.Key);

            await deviceController.Write(deviceInstance, stoppingToken);
        }

        return true;
    }
}
