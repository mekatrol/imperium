using System.Collections.Concurrent;
using Imperium.Common;
using Imperium.Common.Extensions;
using Imperium.Common.Utils;
using Imperium.Models;
using Mekatrol.Devices;

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
        var alfrescoLightUrl = "http://alfresco-light.lan";
        var kitchenViewPowerboardUrl = "http://pbalfresco.home.wojcik.com.au";
        var carportPowerboardUrl = "http://pbcarport.home.wojcik.com.au";

        var alfrescoLightPoints = new PointSet(alfrescoLightUrl);
        alfrescoLightPoints.CreatePoint<PointValue<int>>("Relay", "Alfresco Light", 0);
        var kitchenViewPoints = new PointSet(kitchenViewPowerboardUrl);
        kitchenViewPoints.CreatePoint<PointValue<int>>("Relay1", "String Lights", 0);
        var carportPoints = new PointSet(carportPowerboardUrl);
        carportPoints.CreatePoint<PointValue<int>>("Relay4", "Fish Plant Pump", 0);

        var singleOutputConroller = Services.GetRequiredService<ISingleOutputBoard>();
        var fourOutputController = Services.GetRequiredService<IFourOutputBoard>();

        await singleOutputConroller.Read(alfrescoLightUrl, alfrescoLightPoints, stoppingToken);
        await fourOutputController.Read(kitchenViewPowerboardUrl, kitchenViewPoints, stoppingToken);
        await fourOutputController.Read(carportPowerboardUrl, carportPoints, stoppingToken);

        var allPoints = Services.GetRequiredService<ConcurrentDictionary<string, PointSet>>();

        allPoints[alfrescoLightUrl] = alfrescoLightPoints;
        allPoints[kitchenViewPowerboardUrl] = kitchenViewPoints;
        allPoints[carportPowerboardUrl] = carportPoints;

        /*****************************************************************************
         * START FLOW LOGIC
         *****************************************************************************/

        var now = DateTime.Now;

        // Alfresco on between 19:30 and 06:45
        var alfrescoOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(6, 45));

        // String lights on between 19:30 and 22:30
        var stringOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(22, 30));

        // Fish plant pump on between 07:30 and 19:30
        var fishPlantsOn = now.WithinTimeRange(new TimeOnly(07, 30), new TimeOnly(19, 30));

        var alfrescoLightPoint = alfrescoLightPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay");

        alfrescoLightPoint!.Value = alfrescoOn ? 1 : 0;

        var stringLightPoint = kitchenViewPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay1");

        stringLightPoint!.Value = stringOn ? 1 : 0;

        var fishPlantPump = carportPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay4");

        fishPlantPump!.Value = fishPlantsOn ? 1 : 0;

        /*****************************************************************************
         * END FLOW LOGIC
         *****************************************************************************/


        // Update devices
        try { await singleOutputConroller.Write(alfrescoLightUrl, alfrescoLightPoints, stoppingToken); } catch(Exception ex) { Logger.LogError(ex); }
        try { await fourOutputController.Write(kitchenViewPowerboardUrl, kitchenViewPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }
        try { await fourOutputController.Write(carportPowerboardUrl, carportPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }

        return true;
    }
}
