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

        builder.Services.AddSingleton<IDeviceController, SingleOutputBoard>();

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

        builder.Services.AddTransient<ISingleOutputBoard, SingleOutputBoard>();
        builder.Services.AddTransient<IFourOutputBoard, FourOutputBoard>();

        builder.Services.AddSingleton(new ImperiumState());

        builder.Services.AddHostedService<DeviceControllerBackgroundService>();

        builder.Services.AddSerilog(config =>
        {
            config
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration);
        });

        IList<string> urls = [];
        builder.Configuration.Bind("AppUrls", urls);

        var app = builder.Build();

        InitialiseImporiumState(app.Services);

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

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Listening on URLs: {urls}", $"{string.Join(',', urls)}");

        foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            app.Urls.Add(url);
        }

        app.Run();
    }

    private static ImperiumState InitialiseImporiumState(IServiceProvider services)
    {
        var state = services.GetRequiredService<ImperiumState>();

        var singleOutputBoardController = services.GetRequiredService<ISingleOutputBoard>();
        var fourOutputBoardController = services.GetRequiredService<IFourOutputBoard>();

        state.AddDeviceController(nameof(ISingleOutputBoard), singleOutputBoardController);
        state.AddDeviceController(nameof(IFourOutputBoard), fourOutputBoardController);

        var alfrescoLightUrl = "http://alfresco-light.lan";
        var kitchenViewPowerboardUrl = "http://pbalfresco.home.wojcik.com.au";
        var carportPowerboardUrl = "http://pbcarport.home.wojcik.com.au";

        var alfrescoLight = new DeviceInstance(alfrescoLightUrl, nameof(ISingleOutputBoard));
        alfrescoLight.CreatePoint<PointValue<int>>("Relay", "Alfresco Light", 0);
        var kitchenView = new DeviceInstance(kitchenViewPowerboardUrl, nameof(IFourOutputBoard));
        kitchenView.CreatePoint<PointValue<int>>("Relay1", "String Lights", 0);
        var carport = new DeviceInstance(carportPowerboardUrl, nameof(IFourOutputBoard));
        carport.CreatePoint<PointValue<int>>("Relay4", "Fish Plant Pump", 1);

        state.AddDeviceAndPoints(alfrescoLight);
        state.AddDeviceAndPoints(kitchenView);
        state.AddDeviceAndPoints(carport);

        //var alfrescoLightUrl = "http://alfresco-light.lan";
        //var kitchenViewPowerboardUrl = "http://pbalfresco.home.wojcik.com.au";
        //var carportPowerboardUrl = "http://pbcarport.home.wojcik.com.au";

        //var alfrescoLightPoints = new DeviceInstance(alfrescoLightUrl, alfrescoLightUrl);
        //alfrescoLightPoints.CreatePoint<PointValue<int>>("Relay", "Alfresco Light", 0);
        //var kitchenViewPoints = new DeviceInstance(kitchenViewPowerboardUrl, kitchenViewPowerboardUrl);
        //kitchenViewPoints.CreatePoint<PointValue<int>>("Relay1", "String Lights", 0);
        //var carportPoints = new DeviceInstance(carportPowerboardUrl, carportPowerboardUrl);
        //carportPoints.CreatePoint<PointValue<int>>("Relay4", "Fish Plant Pump", 0);

        //var singleOutputConroller = Services.GetRequiredService<ISingleOutputBoard>();
        //var fourOutputController = Services.GetRequiredService<IFourOutputBoard>();

        //await singleOutputConroller.Read(alfrescoLightUrl, alfrescoLightPoints, stoppingToken);
        //await fourOutputController.Read(kitchenViewPowerboardUrl, kitchenViewPoints, stoppingToken);
        //await fourOutputController.Read(carportPowerboardUrl, carportPoints, stoppingToken);


        //allPoints[alfrescoLightUrl] = alfrescoLightPoints;
        //allPoints[kitchenViewPowerboardUrl] = kitchenViewPoints;
        //allPoints[carportPowerboardUrl] = carportPoints;

        //// Update devices
        //try { await singleOutputConroller.Write(alfrescoLightUrl, alfrescoLightPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }
        //try { await fourOutputController.Write(kitchenViewPowerboardUrl, kitchenViewPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }
        //try { await fourOutputController.Write(carportPowerboardUrl, carportPoints, stoppingToken); } catch (Exception ex) { Logger.LogError(ex); }


        return state;
    }
}
