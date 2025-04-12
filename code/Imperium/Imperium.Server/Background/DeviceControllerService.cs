using System.Collections.Concurrent;
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

        var singleBoardPoints = new PointSet(singleRelayBoardUrl);
        var singleOutputBoard = Services.GetRequiredService<ISingleOutputBoard>();
        await singleOutputBoard.Read(singleRelayBoardUrl, singleBoardPoints, stoppingToken);

        var fourOutputPoints = new PointSet(fourRelayBoardUrl);
        var fourOutputBoard = Services.GetRequiredService<IFourOutputBoard>();
        await fourOutputBoard.Read(fourRelayBoardUrl, fourOutputPoints, stoppingToken);

        var allPoints = Services.GetRequiredService<ConcurrentDictionary<string, PointSet>>();

        allPoints[singleRelayBoardUrl] = singleBoardPoints;
        allPoints[fourRelayBoardUrl] = fourOutputPoints;

        return true;
    }
}
