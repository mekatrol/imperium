using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Server.Background;
using Imperium.Server.DeviceControllers;
using Imperium.Server.Middleware;
using Imperium.Server.Options;
using Imperium.Server.Services;
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

    public static async Task Main(string[] args)
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

        var appVersionService = new AppVersionService();
        builder.Services.AddSingleton<IAppVersionService>(appVersionService);

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

        builder.Services.AddImperiumServices();

        builder.Services.AddSerilog(config =>
        {
            config
                .WriteTo.Console()
                .ReadFrom.Configuration(builder.Configuration);
        });

        var app = builder.Build();

        await InitialiseImperiumState(app.Services);

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

    private static async Task<ImperiumState> InitialiseImperiumState(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        var state = services.GetRequiredService<ImperiumState>();
        var options = services.GetRequiredService<ImperiumStateOptions>();

        // Make sure configuration path exists
        var configurationPath = options.ConfigurationPath;

        var devicesDirectory = Path.Combine(configurationPath, "devices");
        var pointsDirectory = Path.Combine(configurationPath, "points");

        Directory.CreateDirectory(devicesDirectory);
        Directory.CreateDirectory(pointsDirectory);

        // Add virtual point factory and controllers
        var virtualDeviceCotnrollerFactory = new VirtualDeviceControllerFactory();
        virtualDeviceCotnrollerFactory.AddDeviceControllers(services);

        var mekatrolDeviceContollerFactory = new MekatrolDeviceControllerFactory();
        mekatrolDeviceContollerFactory.AddDeviceControllers(services);

        var factories = new Dictionary<string, IDeviceControllerFactory>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in virtualDeviceCotnrollerFactory.GetControllerKeys())
        {
            factories.Add(key, virtualDeviceCotnrollerFactory);
        }

        foreach (var key in mekatrolDeviceContollerFactory.GetControllerKeys())
        {
            factories.Add(key, mekatrolDeviceContollerFactory);
        }

        // Get all device files
        var deviceFiles = Directory.GetFiles(devicesDirectory, "*.json");

        foreach (var deviceFile in deviceFiles)
        {
            var json = await File.ReadAllTextAsync(deviceFile);
            var config = JsonSerializer.Deserialize<DeviceConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions)!;

            if (factories.TryGetValue(config.ControllerKey, out var factory))
            {
                factory.AddDeviceInstance(
                    config.DeviceKey,
                    config.ControllerKey,
                    config.Data,
                    config.Points,
                    state);
            }
            else
            {
                logger.LogWarning("{Message}", $"No factory implmented the controller key '{config.ControllerKey}'.");
            }
        }

        return state;
    }
}
