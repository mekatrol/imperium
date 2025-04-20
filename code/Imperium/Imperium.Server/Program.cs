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

        var mekatrolDeviceContollerFactory = new MekatrolDeviceControllerFactory();
        mekatrolDeviceContollerFactory.AddDeviceControllers(services);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.sunrisesunset",
            "mekatrol.sunrise.sunset.controller",
            "{ \"Url\": \"https://api.sunrise-sunset.org/json?lat=-35.2809&lng=149.1300&date=today&formatted=0\" }",
            [
                new PointDefinition("Sunrise", "Sunrise", typeof(DateTime)),
                new PointDefinition("Sunset", "Sunset", typeof(DateTime)),
                new PointDefinition("SolarNoon", "Solar Noon", typeof(DateTime)),
                new PointDefinition("DayLength", "Day Length", typeof(int)),
                new PointDefinition("CivilTwilightBegin", "Civil Twilight Begin", typeof(DateTime)),
                new PointDefinition("CivilTwilightEnd", "Civil TwilightEnd", typeof(DateTime)),
                new PointDefinition("NauticalTwilightBegin", "Nautical Twilight Begin", typeof(DateTime)),
                new PointDefinition("NauticalTwilightEnd", "Nautical Twilight End", typeof(DateTime)),
                new PointDefinition("AstronomicalTwilightBegin", "Astronomical Twilight Begin", typeof(DateTime)),
                new PointDefinition("AstronomicalTwilightEnd", "Astronomical Twilight End", typeof(DateTime)),
                new PointDefinition("IsDaytime", "Is Daytime", typeof(bool)),
                new PointDefinition("IsNighttime", "Is Nighttime", typeof(bool))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.alfrescolight",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://alfresco-light.lan\" }",
            [
                new PointDefinition("Relay", "Alfresco Light", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.kitchen.light",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://kitchen-cabinet-lights.home.wojcik.com.au\" }",
            [
                new PointDefinition("Relay", "Kitchen Cabinet Light", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.clothesline",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://clothesline-lights.home.wojcik.com.au\" }",
            [
                new PointDefinition("Relay", "Clothes Line Light", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.greenhousepump",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://10.2.2.88\" }",
            [
                new PointDefinition("Relay", "Greenhouse Pump", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.housenumberlight",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://10.2.2.89\" }",
            [
                new PointDefinition("Relay", "House Number", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.frontdoorlight",
            "mekatrol.single.output.controller",
            "{ \"Url\": \"http://10.2.2.90\" }",
            [
                new PointDefinition("Relay", "Front Door Light", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.kitchenview.powerboard",
            "mekatrol.four.output.controller",
            "{ \"Url\": \"http://pbalfresco.home.wojcik.com.au\" }",
            [
                new PointDefinition("Relay1", "String Lights", typeof(int))
            ],
            state);

        mekatrolDeviceContollerFactory.AddDeviceInstance(
            "device.carport.powerboard",
            "mekatrol.four.output.controller",
            "{ \"Url\": \"http://pbcarport.home.wojcik.com.au\" }",
            [
                new PointDefinition("Relay1", "Carport Lights", typeof(int)),
                new PointDefinition("Relay4", "Fish Plant Pump", typeof(int))
            ],
            state);

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
