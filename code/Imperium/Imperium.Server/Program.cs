using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Services;
using Imperium.Server.Background;
using Imperium.Server.DeviceControllers;
using Imperium.Server.Middleware;
using Imperium.Server.Options;
using Imperium.Server.Services;
using Imperium.Server.State;
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

        var kestrelOptions = builder.Configuration.GetSection("Kestrel");

        // Kestrel:Endpoints:WebSocket:Url
        var webSocketUrl = kestrelOptions.GetSection("Endpoints:WebSocket:Url").Value;

        if (string.IsNullOrEmpty(webSocketUrl))
        {
            throw new Exception("Configuration section 'Endpoints:WebSocket:Url' missing or the value is empty.");
        }

        if (!Uri.IsWellFormedUriString(webSocketUrl, UriKind.Absolute))
        {
            throw new Exception($"Configuration section 'Endpoints:WebSocket:Url' value '{webSocketUrl}' is not a wel formed absolute URL.");
        }

        // Convert to web socket URL
        var webSocketUri = new Uri(webSocketUrl);

        builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Configure(kestrelOptions);
                });

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
        var timerOptions = new TimerBackgroundServiceOptions();
        builder.Configuration.Bind(TimerBackgroundServiceOptions.SectionName, timerOptions);
        builder.Services.AddSingleton(timerOptions);

        var deviceControllerOptions = new DeviceControllerBackgroundServiceOptions();
        builder.Configuration.Bind(DeviceControllerBackgroundServiceOptions.SectionName, deviceControllerOptions);
        builder.Services.AddSingleton(deviceControllerOptions);

        var flowExecutorControllerOptions = new FlowExecutorBackgroundServiceOptions();
        builder.Configuration.Bind(FlowExecutorBackgroundServiceOptions.SectionName, flowExecutorControllerOptions);
        builder.Services.AddSingleton(flowExecutorControllerOptions);

        builder.Services.AddSingleton(new MqttHostConfiguration());

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

        InitialiseConfiguration(builder.Services, imperiumStateConfig);

        var imperiumState = new ImperiumState
        {
            IsReadOnlyMode = imperiumStateConfig.IsReadOnlyMode,
            WebSocketUri = webSocketUri
        };

        builder.Services.AddSingleton(imperiumState);
        builder.Services.AddSingleton<IPointState>(imperiumState);
        builder.Services.AddSingleton<IImperiumState>(imperiumState);
        builder.Services.AddSingleton<IDeviceInstanceFactory, DeviceInstanceFactory>();

        builder.Services.AddTransient<WebSocketMiddleware>();

        builder.Services.AddHostedService<TimerBackgroundService>();
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

        var cancennationTokenSourceService = app.Services.GetRequiredService<ICancellationTokenSourceService>();
        var webSocketManager = app.Services.GetRequiredService<IWebSocketClientManagerService>();
        app.Lifetime.ApplicationStopping.Register(async () =>
        {
            cancennationTokenSourceService.CancelAll();
            await webSocketManager.CloseAll();
        });

        using (var cancellationTokenSouce = new CancellationTokenSource())
        {
            await StatePersistor.LoadState(app.Services, cancellationTokenSouce.Token);
        }

        app.UseExceptionMiddleware();

        app.UseWebSockets();
        app.UseMiddleware<WebSocketMiddleware>();

        app.UseMiddleware<PrivateNetworkCorsHeaderMiddleware>();

        app.UseCors(AppCorsPolicy);

        // Middleware for injecting API base URL into index.html
        app.UseInjectApiBaseUrl(app.Environment);

        app.UseStaticFiles();
        app.UseDefaultFiles();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.EnableTryItOutByDefault();
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static void InitialiseConfiguration(IServiceCollection services, ImperiumStateOptions config)
    {
        var imperiumDirectories = new ImperiumDirectories(config.ConfigurationPath);
        services.AddSingleton(imperiumDirectories);

        Directory.CreateDirectory(imperiumDirectories.Devices);
        Directory.CreateDirectory(imperiumDirectories.Scripts);
    }
}
