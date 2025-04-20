using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Server.Background;
using Imperium.Server.Middleware;
using Imperium.Server.Options;
using Imperium.Server.State;
using Mekatrol.Devices;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imperium.Server;

public class Program
{
    private const string AppCorsPolicy = nameof(AppCorsPolicy);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });

        // Bind origins options
        var originsOptions = new OriginsOptions();
        builder.Configuration.Bind(OriginsOptions.SectionName, originsOptions);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: AppCorsPolicy,
                policy =>
                {
                    policy.WithOrigins([.. originsOptions]);
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
                });
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
        var deviceControllerOptions = new DeviceControllerBackgroundServiceOptions();
        builder.Configuration.Bind(DeviceControllerBackgroundServiceOptions.SectionName, deviceControllerOptions);
        builder.Services.AddSingleton(deviceControllerOptions);

        var flowExecutorControllerOptions = new FlowExecutorBackgroundServiceOptions();
        builder.Configuration.Bind(FlowExecutorBackgroundServiceOptions.SectionName, flowExecutorControllerOptions);
        builder.Services.AddSingleton(flowExecutorControllerOptions);

        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = httpClientOptions.ConnectTimeout,
            PooledConnectionLifetime = httpClientOptions.ConnectionLifeTime,
            ResponseDrainTimeout = httpClientOptions.ResponseDrainTimeout
        };
        var client = new HttpClient(handler)
        {
            Timeout = httpClientOptions.Timeout
        };

        builder.Services.AddSingleton(client);

        builder.Services.AddHttpClient(nameof(HttpClient), client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = httpClientOptions.Timeout;

        });

        var imperiumStateConfig = new ImperiumStateOptions();
        builder.Configuration.Bind(ImperiumStateOptions.SectionName, imperiumStateConfig);
        builder.Services.AddSingleton(imperiumStateConfig);

        var imperiumState = new ImperiumState
        {
            IsReadOnlyMode = imperiumStateConfig.IsReadOnlyMode,
            MqttServer = imperiumStateConfig.MqttServer,
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

        app.UseCors(AppCorsPolicy);

        app.UseExceptionMiddleware();

        // Middleware for injecting API base URL into index.html
        app.UseInjectApiBaseUrl(app.Environment);

        app.UseDefaultFiles();
        app.UseStaticFiles();

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

        var kitchen = new DeviceInstance<ControllerConfiguration>(
            "device.kitchen.light",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://kitchen-cabinet-lights.home.wojcik.com.au" });
        kitchen.CreatePoint<int>("Relay", "Kitchen Cabinet Light");

        var clothesLineLight = new DeviceInstance<ControllerConfiguration>(
            "device.clothesline",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://clothesline-lights.home.wojcik.com.au" });
        clothesLineLight.CreatePoint<int>("Relay", "Clothes Line Light");

        var greenhousePump = new DeviceInstance<ControllerConfiguration>(
            "device.alfrescolight",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://10.2.2.88" });
        greenhousePump.CreatePoint<int>("Relay", "Greenhouse Pump");

        var houseNumberLight = new DeviceInstance<ControllerConfiguration>(
            "device.housenumberlight",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://10.2.2.89" });
        houseNumberLight.CreatePoint<int>("Relay", "House Number");

        var frontDoorLight = new DeviceInstance<ControllerConfiguration>(
            "device.frontdoorlight",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://10.2.2.90" });
        frontDoorLight.CreatePoint<int>("Relay", "Front Door Light");

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
        carport.CreatePoint<int>("Relay1", "Carport Lights");

        state.AddDeviceAndPoints(sunriseSunset);
        state.AddDeviceAndPoints(alfrescoLight);
        state.AddDeviceAndPoints(clothesLineLight);
        state.AddDeviceAndPoints(kitchenView);
        state.AddDeviceAndPoints(carport);
        state.AddDeviceAndPoints(kitchen);
        state.AddDeviceAndPoints(houseNumberLight);
        state.AddDeviceAndPoints(frontDoorLight);

        AddHouseAlarmPoint(1, "Lounge room", state);
        AddHouseAlarmPoint(2, "Dining room", state);
        AddHouseAlarmPoint(3, "Bedroom 1", state);
        AddHouseAlarmPoint(4, "Bedroom 2", state);
        AddHouseAlarmPoint(5, "Bedroom 3", state);
        AddHouseAlarmPoint(6, "Bedroom 4", state);
        AddHouseAlarmPoint(7, "Front door", state);
        AddHouseAlarmPoint(8, "Back door", state);

        AddVirtualPoint("kitchen.light.timer", PointType.DateTime, "Kitchen light timer", state);
        AddVirtualPoint("water.pumps", PointType.Boolean, "Water Pumps", state);
        AddVirtualPoint("panic", PointType.Boolean, "Panic", state);

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

    private static void AddVirtualPoint(string key, PointType type, string friendlyName, ImperiumState state)
    {
        // Get with default value
        var point = new Point(key, type)
        {
            // Set friendly name
            FriendlyName = friendlyName
        };

        state.AddPoint("virtual", point);
    }
}
