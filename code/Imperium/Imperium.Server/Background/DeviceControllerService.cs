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
        var singleRelayBoardUrl = "http://alfresco-light.lan";
        var fourRelayBoardUrl = "http://pbalfresco.home.wojcik.com.au";
        var carportRelayBoardUrl = "http://pbcarport.home.wojcik.com.au";

        var singleBoardPoints = new PointSet(singleRelayBoardUrl);
        singleBoardPoints.CreatePoint<PointValue<int>>("Relay", "Alfresco Light", 0);

        var singleOutputBoard = Services.GetRequiredService<ISingleOutputBoard>();
        await singleOutputBoard.Read(singleRelayBoardUrl, singleBoardPoints, stoppingToken);

        var fourOutputPoints = new PointSet(fourRelayBoardUrl);
        fourOutputPoints.CreatePoint<PointValue<int>>("Relay1", "String Lights", 0);

        var fourOutputBoard = Services.GetRequiredService<IFourOutputBoard>();
        await fourOutputBoard.Read(fourRelayBoardUrl, fourOutputPoints, stoppingToken);

        var carportOutputPoints = new PointSet(carportRelayBoardUrl);
        carportOutputPoints.CreatePoint<PointValue<int>>("Relay4", "Fish Plant Pump", 0);
        var carportOutputBoard = Services.GetRequiredService<IFourOutputBoard>();
        await carportOutputBoard.Read(carportRelayBoardUrl, carportOutputPoints, stoppingToken);

        var allPoints = Services.GetRequiredService<ConcurrentDictionary<string, PointSet>>();

        allPoints[singleRelayBoardUrl] = singleBoardPoints;
        allPoints[fourRelayBoardUrl] = fourOutputPoints;
        allPoints[carportRelayBoardUrl] = carportOutputPoints;

        /*****************************************************************************
         * START FLOW LOGIC
         *****************************************************************************/

        var now = DateTime.Now;

        // Alfresco on between 19:30 and 07:30
        var alfrescoOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(7, 30));

        // Alfresco on between 19:30 and 22:30
        var stringOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(22, 30));

        // Fish plant pump on between 07:30 and 19:30
        var fishPlantsOn = now.WithinTimeRange(new TimeOnly(07, 30), new TimeOnly(19, 30));

        var alfrescoLightPoint = singleBoardPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay");

        alfrescoLightPoint!.Value = alfrescoOn ? 1 : 0;

        var stringLightPoint = fourOutputPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay1");

        stringLightPoint!.Value = stringOn ? 1 : 0;

        var fishPlantPump = carportOutputPoints.Points
            .Cast<PointValue<int>>()
            .SingleOrDefault(x => x.Id == "Relay4");

        fishPlantPump!.Value = fishPlantsOn ? 1 : 0;

        /*****************************************************************************
         * END FLOW LOGIC
         *****************************************************************************/


        // Update devices
        try { await singleOutputBoard.Write(singleRelayBoardUrl, singleBoardPoints, stoppingToken); } catch(Exception ex) { Logger.LogError(ex); }
        try { await fourOutputBoard.Write(fourRelayBoardUrl, fourOutputPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }
        try { await carportOutputBoard.Write(carportRelayBoardUrl, carportOutputPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }

        return true;
    }
}
