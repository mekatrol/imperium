using Imperium.Common.Devices;
using Imperium.Common.Points;
using Imperium.Server.Background;
using Imperium.Server.Middleware;
using Imperium.Server.Options;
using Imperium.Server.State;
using Mekatrol.Devices;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Imperium.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddExceptionMiddleware();

        builder.Services.AddSingleton<IDeviceController, SingleOutputController>();

        // Bind http client options
        var httpClientOptions = new HttpClientOptions();
        builder.Configuration.Bind(HttpClientOptions.SectionName, httpClientOptions);
        builder.Services.AddSingleton(httpClientOptions);

        // Bind background service options        
        var backgroundServiceOptions = new BackgroundServiceOptions();
        builder.Configuration.Bind(BackgroundServiceOptions.SectionName, backgroundServiceOptions);
        builder.Services.AddSingleton(backgroundServiceOptions);

        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = httpClientOptions.ConnectionLifeTime
        };
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        builder.Services.AddSingleton(client);

        var imperiumStateConfig = new ImperiumStateOptions();
        builder.Configuration.Bind(ImperiumStateOptions.SectionName, imperiumStateConfig);
        builder.Services.AddSingleton(imperiumStateConfig);

        var imperiumState = new ImperiumState
        {
            IsReadOnlyMode = imperiumStateConfig.IsReadOnlyMode,
            MqttUser = imperiumStateConfig.MqttUser,
            MqttPassword = imperiumStateConfig.MqttPassword
        };

        builder.Services.AddSingleton(imperiumState);
        builder.Services.AddSingleton<IPointState>(imperiumState);
        builder.Services.AddSingleton<IImperiumState>(imperiumState);

        builder.Services.AddHostedService<DeviceControllerBackgroundService>();
        builder.Services.AddHostedService<FlowExecutorBackgroundService>();
        builder.Services.AddHostedService<MqttClientBackgroundService>();

        builder.Services.AddSerilog(config =>
        {
            config
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration);
        });

        var app = builder.Build();

        InitialiseImperiumState(app.Services);

        app.UseExceptionMiddleware();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnableTryItOutByDefault();
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        if (!app.Environment.IsDevelopment())
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Listening on URLs: {urls}", $"{string.Join(',', imperiumStateConfig.ApplicationUrls)}");

            foreach (var url in imperiumStateConfig.ApplicationUrls)
            {
                app.Urls.Add(url);
            }
        }

        app.Run();
    }

    private static ImperiumState InitialiseImperiumState(IServiceProvider services)
    {
        var state = services.GetRequiredService<ImperiumState>();
        var pointState = services.GetRequiredService<IPointState>();

        var sunriseSunsetController = new SunriseSunsetController(
            services.GetRequiredService<HttpClient>(),
            pointState,
            services.GetRequiredService<ILogger<SunriseSunsetController>>());

        var singleOutputBoardController = new SingleOutputController(
            services.GetRequiredService<HttpClient>(),
            pointState,
            services.GetRequiredService<ILogger<SingleOutputController>>());

        var fourOutputBoardController = new FourOutputController(
            services.GetRequiredService<HttpClient>(),
            pointState,
            services.GetRequiredService<ILogger<FourOutputController>>());

        state.AddDeviceController(nameof(SunriseSunsetController), sunriseSunsetController);
        state.AddDeviceController(nameof(ISingleOutputController), singleOutputBoardController);
        state.AddDeviceController(nameof(IFourOutputController), fourOutputBoardController);

        var sunriseSunset = new DeviceInstance<ControllerConfiguration>(
            "device.sunrise.sunset",
            nameof(SunriseSunsetController),
            new ControllerConfiguration { Url = "https://api.sunrise-sunset.org/json?lat=-35.2809&lng=149.1300&date=today&formatted=0" });
        sunriseSunset.CreatePoint<DateTime>("Sunrise", "Sunrise");
        sunriseSunset.CreatePoint<DateTime>("Sunset", "Sunset");
        sunriseSunset.CreatePoint<DateTime>("SolarNoon", "Solar Noon");
        sunriseSunset.CreatePoint<int>("DayLength", "Day Length");
        sunriseSunset.CreatePoint<DateTime>("CivilTwilightBegin", "Civil Twilight Begin");
        sunriseSunset.CreatePoint<DateTime>("CivilTwilightEnd", "Civil TwilightEnd");
        sunriseSunset.CreatePoint<DateTime>("NauticalTwilightBegin", "Nautical Twilight Begin");
        sunriseSunset.CreatePoint<DateTime>("NauticalTwilightEnd", "Nautical Twilight End");
        sunriseSunset.CreatePoint<DateTime>("AstronomicalTwilightBegin", "Astronomical Twilight Begin");
        sunriseSunset.CreatePoint<DateTime>("AstronomicalTwilightEnd", "Astronomical Twilight End");
        sunriseSunset.CreatePoint<bool>("IsDaytime", "Is Daytime");
        sunriseSunset.CreatePoint<bool>("IsNighttime", "Is Nighttime");

        var alfrescoLight = new DeviceInstance<ControllerConfiguration>(
            "device.alfrescolight",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://alfresco-light.lan" });
        alfrescoLight.CreatePoint<int>("Relay", "Alfresco Light");

        var kitchenView = new DeviceInstance<ControllerConfiguration>(
            "device.kitchenview.powerboard",
            nameof(IFourOutputController),
            new ControllerConfiguration { Url = "http://pbalfresco.home.wojcik.com.au" });
        kitchenView.CreatePoint<int>("Relay1", "String Lights");

        var carport = new DeviceInstance<ControllerConfiguration>(
            "device.carport.powerboard",
            nameof(IFourOutputController),
            new ControllerConfiguration { Url = "http://pbcarport.home.wojcik.com.au" });
        carport.CreatePoint<int>("Relay4", "Fish Plant Pump");

        state.AddDeviceAndPoints(sunriseSunset);
        state.AddDeviceAndPoints(alfrescoLight);
        state.AddDeviceAndPoints(kitchenView);
        state.AddDeviceAndPoints(carport);

        AddHouseAlarmPoint(1, "Lounge room", state);
        AddHouseAlarmPoint(2, "Dining room", state);
        AddHouseAlarmPoint(3, "Bedroom 1", state);
        AddHouseAlarmPoint(4, "Bedroom 2", state);
        AddHouseAlarmPoint(5, "Bedroom 3", state);
        AddHouseAlarmPoint(6, "Bedroom 4", state);
        AddHouseAlarmPoint(7, "Front door", state);
        AddHouseAlarmPoint(8, "Back door", state);

        return state;
    }

    private static void AddHouseAlarmPoint(int zone, string friendlyName, ImperiumState state)
    {
        // Get with default value
        var point = new Point($"zone{zone}", PointType.String)
        {
            // Set friendly name
            FriendlyName = friendlyName
        };

        state.AddPoint("housealarm", point);
    }
}
