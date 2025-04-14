using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Imperium.Common;
using Imperium.Models;
using Imperium.Server.Background;
using Mekatrol.Devices;
using Serilog;

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

        var imperiumStateConfig = new ImperiumStateConfig();
        builder.Configuration.Bind(ImperiumStateConfig.SectionName, imperiumStateConfig);
        builder.Services.AddSingleton(imperiumStateConfig);

        var imperiumState = new ImperiumState
        {
            IsReadOnlyMode = imperiumStateConfig.IsReadOnlyMode
        };
        builder.Services.AddSingleton(imperiumState);
        builder.Services.AddSingleton<IPointState>(imperiumState);

        builder.Services.AddHostedService<DeviceControllerBackgroundService>();
        builder.Services.AddHostedService<FlowExecutorBackgroundService>();

        builder.Services.AddSerilog(config =>
        {
            config
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration);
        });

        var app = builder.Build();

        InitialiseImperiumState(app.Services);

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnableTryItOutByDefault();
        });
        //}

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
        sunriseSunset.CreatePoint("Sunrise", "Sunrise", DateTime.MinValue);
        sunriseSunset.CreatePoint("Sunset", "Sunset", DateTime.MinValue);
        sunriseSunset.CreatePoint("SolarNoon", "Solar Noon", DateTime.MinValue);
        sunriseSunset.CreatePoint("DayLength", "Day Length", 0);
        sunriseSunset.CreatePoint("CivilTwilightBegin", "Civil Twilight Begin", DateTime.MinValue);
        sunriseSunset.CreatePoint("CivilTwilightEnd", "Civil TwilightEnd", DateTime.MinValue);
        sunriseSunset.CreatePoint("NauticalTwilightBegin", "Nautical Twilight Begin", DateTime.MinValue);
        sunriseSunset.CreatePoint("NauticalTwilightEnd", "Nautical Twilight End", DateTime.MinValue);
        sunriseSunset.CreatePoint("AstronomicalTwilightBegin", "Astronomical Twilight Begin", DateTime.MinValue);
        sunriseSunset.CreatePoint("AstronomicalTwilightEnd", "Astronomical Twilight End", DateTime.MinValue);
        sunriseSunset.CreatePoint("IsDaytime", "Is Daytime", true);
        sunriseSunset.CreatePoint("IsNighttime", "Is Nighttime", false);

        var alfrescoLight = new DeviceInstance<ControllerConfiguration>(
            "device.alfrescolight",
            nameof(ISingleOutputController),
            new ControllerConfiguration { Url = "http://alfresco-light.lan" });
        alfrescoLight.CreatePoint("Relay", "Alfresco Light", 0);

        var kitchenView = new DeviceInstance<ControllerConfiguration>(
            "device.kitchenview.powerboard",
            nameof(IFourOutputController),
            new ControllerConfiguration { Url = "http://pbalfresco.home.wojcik.com.au" });
        kitchenView.CreatePoint("Relay1", "String Lights", 0);

        var carport = new DeviceInstance<ControllerConfiguration>(
            "device.carport.powerboard",
            nameof(IFourOutputController),
            new ControllerConfiguration { Url = "http://pbcarport.home.wojcik.com.au" });
        carport.CreatePoint("Relay4", "Fish Plant Pump", 1);

        state.AddDeviceAndPoints(sunriseSunset);
        state.AddDeviceAndPoints(alfrescoLight);
        state.AddDeviceAndPoints(kitchenView);
        state.AddDeviceAndPoints(carport);

        return state;
    }
}
