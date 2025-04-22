using Imperium.Common.Controllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mekatrol.Devices;

public class MekatrolDeviceControllerFactory : IDeviceControllerFactory
{
    private const string SunriseSunsetControllerKey = "mekatrol.sunrise.sunset.controller";
    private const string SingleOutputControllerKey = "mekatrol.single.output.controller";
    private const string FourOutputControllerKey = "mekatrol.four.output.controller";

    public void AddDeviceControllers(IServiceProvider services)
    {
        var state = services.GetRequiredService<IImperiumState>();
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
    }

    public IDeviceInstance AddDeviceInstance(
        string deviceKey,
        string controllerKey,
        string configurationJson,
        IList<PointDefinition> points,
        IImperiumState state)
    {
        switch (controllerKey.ToLower())
        {
            case SunriseSunsetControllerKey:
                return AddDevice<SunriseSunsetResultsModel>(deviceKey, controllerKey, configurationJson, points, state);

            case SingleOutputControllerKey:
                return AddDevice<SingleOutputControllerModel>(deviceKey, controllerKey, configurationJson, points, state);

            case FourOutputControllerKey:
                return AddDevice<FourOutputControllerModel>(deviceKey, controllerKey, configurationJson, points, state);

            default:
                throw new ArgumentException($"{nameof(MekatrolDeviceControllerFactory)} does not implement a controller with the key '{controllerKey}'.");
        }
    }

    private static IDeviceInstance AddDevice<TModel>(string deviceKey,
        string controllerKey,
        string configurationJson,
        IList<PointDefinition> points,
        IImperiumState state)
    {
        // Get the controller
        var controller = state.GetDeviceController(controllerKey);

        if (controller == null)
        {
            throw new Exception($"A controller with the key '{controllerKey}' has not been added. Please call '{nameof(MekatrolDeviceControllerFactory)}.{nameof(AddDeviceControllers)}' before calling this method.");
        }

        var instanceConfiguration = JsonSerializer.Deserialize<InstanceConfiguration>(configurationJson);

        // get list of valid points based on model
        var pointProperties = typeof(TModel).GetProperties()
            .ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

        var deviceInstance = new DeviceInstance<InstanceConfiguration>(
           deviceKey,
           controllerKey,
           instanceConfiguration);

        foreach (var point in points)
        {
            if (!pointProperties.TryGetValue(point.Key, out var pointProperty))
            {
                throw new Exception($"The point key '{point.Key}' is not valid for the controller '{controllerKey}'.");
            }

            var nativePointType = point.PointType.GetPointNativeType();

            if(nativePointType != pointProperty.PropertyType)
            {
                // Points of type 'Boolean' can be mapped to types of 'int' in Mekatrol controllers because
                // Mekatrol controlers treat '0 == false' and '!0 == true'
                if(nativePointType != typeof(bool) || pointProperty.PropertyType != typeof(int))
                {
                    throw new Exception($"The point with key '{point.Key}' has type '{point.PointType}' defined which is not compatible for the controller '{controllerKey}' with point type '{pointProperty.PropertyType}'.");
                }
            }

            deviceInstance.MapPoint(point.Key, point.FriendlyName, nativePointType, null);
        }

        state.AddDeviceAndPoints(deviceInstance);

        return deviceInstance;
    }
}
