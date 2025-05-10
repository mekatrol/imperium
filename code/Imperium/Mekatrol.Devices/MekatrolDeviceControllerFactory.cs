using Imperium.Common.DeviceControllers;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public class MekatrolDeviceControllerFactory : IDeviceControllerFactory
{
    private const string SunriseSunsetControllerKey = "mekatrol.sunrise.sunset.controller";
    private const string SingleOutputControllerKey = "mekatrol.single.output.controller";
    private const string FourOutputControllerKey = "mekatrol.four.output.controller";

    public IDeviceController? AddDeviceController(IServiceProvider services, string controllerKey)
    {
        var state = services.GetRequiredService<IImperiumState>();
        var pointState = services.GetRequiredService<IPointState>();

        switch (controllerKey)
        {
            case SunriseSunsetControllerKey:
                var sunriseSunsetController = new SunriseSunsetController(
                    services.GetRequiredService<IHttpClientFactory>(),
                    pointState,
                    services.GetRequiredService<ILogger<SunriseSunsetController>>());

                state.AddDeviceController(SunriseSunsetControllerKey, sunriseSunsetController);

                return sunriseSunsetController;

            case SingleOutputControllerKey:
                var singleOutputBoardController = new SingleOutputController(
                    services.GetRequiredService<IHttpClientFactory>(),
                    pointState,
                    services.GetRequiredService<ILogger<SingleOutputController>>());

                state.AddDeviceController(SingleOutputControllerKey, singleOutputBoardController);

                return singleOutputBoardController;

            case FourOutputControllerKey:
                var fourOutputBoardController = new FourOutputController(
                    services.GetRequiredService<IHttpClientFactory>(),
                    pointState,
                    services.GetRequiredService<ILogger<FourOutputController>>());

                state.AddDeviceController(FourOutputControllerKey, fourOutputBoardController);

                return fourOutputBoardController;

            default:
                // Null means the device controller key is not valid
                return null;
        }
    }
}
