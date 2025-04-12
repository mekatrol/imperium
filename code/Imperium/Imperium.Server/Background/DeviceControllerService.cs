using System.Collections.Concurrent;
using System.Numerics;
using Imperium.Common;
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
        var singleOutputBoard = Services.GetRequiredService<ISingleOutputBoard>();
        await singleOutputBoard.Read(singleRelayBoardUrl, singleBoardPoints, stoppingToken);

        var fourOutputPoints = new PointSet(fourRelayBoardUrl);
        var fourOutputBoard = Services.GetRequiredService<IFourOutputBoard>();
        await fourOutputBoard.Read(fourRelayBoardUrl, fourOutputPoints, stoppingToken);

        var carportOutputPoints = new PointSet(carportRelayBoardUrl);
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

        var alfrescoOn = (now.Hour >= 19 && now.Minute > 30) || now.Hour > 19 || now.Hour <= 6 || (now.Hour < 8 && now.Minute < 30);
        var stringOn = (now.Hour == 19 && now.Minute > 30) || now.Hour == 20 || now.Hour == 21 || (now.Hour == 22 && now.Minute < 33);
        var fishPlantsOn = (now.Hour == 7 && now.Minute > 30) || (now.Hour > 7 && now.Hour < 21);

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
        await singleOutputBoard.Write(singleRelayBoardUrl, singleBoardPoints, stoppingToken);
        await fourOutputBoard.Write(fourRelayBoardUrl, fourOutputPoints, stoppingToken);
        await carportOutputBoard.Write(carportRelayBoardUrl, carportOutputPoints, stoppingToken);

        return true;
    }
}
