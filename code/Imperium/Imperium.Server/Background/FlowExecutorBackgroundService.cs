using Imperium.Common.Points;
using Imperium.Common.Utils;
using Imperium.Server.Options;

namespace Imperium.Server.Background;

internal class FlowExecutorBackgroundService(
    FlowExecutorBackgroundServiceOptions backgroundServiceOptions,
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

        var isNighttime = (bool?)pointState.GetPointValue("device.sunrisesunset", "IsNighttime");

        if (isNighttime.HasValue)
        {
            // Alfresco on when night time
            pointState.UpdatePointValue("device.alfrescolight", "Relay", isNighttime.Value ? 1 : 0, PointValueType.Control);
        }

        // Set panic off for now
        pointState.UpdatePointValue("virtual", "panic", false, PointValueType.Device);

        // String lights on between 19:30 and 22:30
        var stringOn = now.WithinTimeRange(new TimeOnly(18, 00), new TimeOnly(22, 30));
        pointState.UpdatePointValue("device.kitchenview.powerboard", "Relay1", stringOn ? 1 : 0, PointValueType.Control);

        // Fish plant pump on between 07:30 and 19:30
        var fishPlantsOn = now.WithinTimeRange(new TimeOnly(07, 30), new TimeOnly(19, 30));
        pointState.UpdatePointValue("device.carport.powerboard", "Relay4", fishPlantsOn ? 1 : 0, PointValueType.Control);

        // Pumps are on (this is a virtual point, Imperium is the device so set point valur type to device).
        pointState.UpdatePointValue("virtual", "water.pumps", fishPlantsOn, PointValueType.Device);

        var alarmZone2Value = (string?)pointState.GetPointValue("housealarm", "zone2");

        var timer = pointState.GetDevicePoint("virtual", "kitchen.light.timer");
        if ("EVT_UNSEALED" == alarmZone2Value && isNighttime.HasValue && isNighttime.Value)
        {
            pointState.UpdatePointValue("device.kitchen.light", "Relay", 1, PointValueType.Control);

            if (timer != null)
            {
                // Set to expire in two minutes from now
                pointState.UpdatePointValue("virtual", "kitchen.light.timer", DateTime.Now + new TimeSpan(0, 5, 0), PointValueType.Control);
            }
        }
        else if (timer != null)
        {
            var expiry = pointState.GetPointValue<DateTime?>("virtual", "kitchen.light.timer");

            if (expiry != null && expiry <= DateTime.Now)
            {
                // Clear timer
                pointState.UpdatePointValue("virtual", "kitchen.light.timer", null, PointValueType.Control);

                // Turn off kitchen light
                pointState.UpdatePointValue("device.kitchen.light", "Relay", 0, PointValueType.Control);
            }
            else if (expiry == null)
            {
                // Turn off kitchen light
                pointState.UpdatePointValue("device.kitchen.light", "Relay", 0, PointValueType.Control);
            }
        }

        /*****************************************************************************
         * END FLOW LOGIC
         *****************************************************************************/

        return await Task.FromResult(true);
    }
}
