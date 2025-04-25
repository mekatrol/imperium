using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mekatrol.Devices;

public static class MekatrolDeviceControllerFactory
{
    private const string SunriseSunsetControllerKey = "mekatrol.sunrise.sunset.controller";
    private const string SingleOutputControllerKey = "mekatrol.single.output.controller";
    private const string FourOutputControllerKey = "mekatrol.four.output.controller";

    public static IImperiumState AddMekatrolDeviceControllers(this IImperiumState state, IServiceProvider services)
    {
        var pointState = services.GetRequiredService<IPointState>();

        var sunriseSunsetController = new SunriseSunsetController(
            services.GetRequiredService<IHttpClientFactory>(),
            pointState,
            services.GetRequiredService<ILogger<SunriseSunsetController>>());

        var singleOutputBoardController = new SingleOutputController(
            services.GetRequiredService<IHttpClientFactory>(),
            pointState,
            services.GetRequiredService<ILogger<SingleOutputController>>());

        var fourOutputBoardController = new FourOutputController(
            services.GetRequiredService<IHttpClientFactory>(),
            pointState,
            services.GetRequiredService<ILogger<FourOutputController>>());

        state.AddDeviceController(SunriseSunsetControllerKey, sunriseSunsetController);
        state.AddDeviceController(SingleOutputControllerKey, singleOutputBoardController);
        state.AddDeviceController(FourOutputControllerKey, fourOutputBoardController);

        return state;
    }
}
