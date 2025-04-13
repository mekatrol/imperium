using Imperium.Common.Utils;
using Imperium.Models;

namespace Imperium.Server.Background;

internal class FlowExecutorBackgroundService(
    BackgroundServiceOptions backgroundServiceOptions,
    IServiceProvider serviceProvider,
    ILogger<FlowExecutorBackgroundService> logger)
    : BaseBackgroundService<FlowExecutorBackgroundService>(
        backgroundServiceOptions,
        serviceProvider,
        logger)
{
    protected override async Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var now = DateTime.Now;
        var pointState = Services.GetRequiredService<IPointState>();

        /*****************************************************************************
         * START FLOW LOGIC
         *****************************************************************************/

        // Alfresco on between 19:30 and 06:45
        var alfrescoOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(6, 45));
        pointState.UpdatePointValue("device.alfrescolight", "Relay", alfrescoOn ? 1 : 0);

        // String lights on between 19:30 and 22:30
        var stringOn = now.WithinTimeRange(new TimeOnly(19, 30), new TimeOnly(22, 30));
        pointState.UpdatePointValue("device.kitchenview.powerboard", "Relay1", stringOn ? 1 : 0);

        // Fish plant pump on between 07:30 and 19:30
        var fishPlantsOn = now.WithinTimeRange(new TimeOnly(07, 30), new TimeOnly(19, 30));
        pointState.UpdatePointValue("device.carport.powerboard", "Relay4", fishPlantsOn ? 1 : 0);

        /*****************************************************************************
         * END FLOW LOGIC
         *****************************************************************************/

        return await Task.FromResult(true);
    }
}