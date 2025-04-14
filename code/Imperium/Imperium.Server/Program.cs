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

        var singleOutputBoardController = new SingleOutputController(
            services.GetRequiredService<HttpClient>(),
            services.GetRequiredService<ILogger<SingleOutputController>>());

        var fourOutputBoardController = new FourOutputController(
            services.GetRequiredService<HttpClient>(),
            services.GetRequiredService<ILogger<FourOutputController>>());

        state.AddDeviceController(nameof(ISingleOutputController), singleOutputBoardController);
        state.AddDeviceController(nameof(IFourOutputController), fourOutputBoardController);

        var alfrescoLight = new DeviceInstance<OutputControllerConfiguration>(
            "device.alfrescolight",
            nameof(ISingleOutputController),
            new OutputControllerConfiguration { Url = "http://alfresco-light.lan" });
        alfrescoLight.CreatePoint<int>("Relay", "Alfresco Light", 0);

        var kitchenView = new DeviceInstance<OutputControllerConfiguration>(
            "device.kitchenview.powerboard",
            nameof(IFourOutputController),
            new OutputControllerConfiguration { Url = "http://pbalfresco.home.wojcik.com.au" });
        kitchenView.CreatePoint<int>("Relay1", "String Lights", 0);

        var carport = new DeviceInstance<OutputControllerConfiguration>(
            "device.carport.powerboard",
            nameof(IFourOutputController),
            new OutputControllerConfiguration { Url = "http://pbcarport.home.wojcik.com.au" });
        carport.CreatePoint<int>("Relay4", "Fish Plant Pump", 1);

        state.AddDeviceAndPoints(alfrescoLight);
        state.AddDeviceAndPoints(kitchenView);
        state.AddDeviceAndPoints(carport);

        return state;
    }
}
