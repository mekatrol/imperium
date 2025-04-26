using Imperium.Common;
using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Status;
using Imperium.ScriptCompiler;
using Imperium.Server.Background;
using Imperium.Server.DeviceControllers;
using Imperium.Server.Middleware;
using Imperium.Server.Options;
using Imperium.Server.Services;
using Imperium.Server.State;
using Mekatrol.Devices;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;
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

        InitialiseConfiguration(builder.Services, imperiumStateConfig);

        var imperiumState = new ImperiumState
        {
            IsReadOnlyMode = imperiumStateConfig.IsReadOnlyMode
        };

        builder.Services.AddSingleton(imperiumState);
        builder.Services.AddSingleton<IPointState>(imperiumState);
        builder.Services.AddSingleton<IImperiumState>(imperiumState);
        builder.Services.AddSingleton<IDeviceControllerFactory, DeviceControllerFactory>();

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

        using (var cancellationTokenSouce = new CancellationTokenSource())
        {
            await InitialiseImperiumState(app.Services, cancellationTokenSouce.Token);
        }

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

    private static void InitialiseConfiguration(IServiceCollection services, ImperiumStateOptions config)
    {
        var imperiumDirectories = new ImperiumDirectories(config.ConfigurationPath);
        services.AddSingleton(imperiumDirectories);

        Directory.CreateDirectory(imperiumDirectories.Devices);
        Directory.CreateDirectory(imperiumDirectories.Points);
        Directory.CreateDirectory(imperiumDirectories.Scripts);
    }

    private static async Task<ImperiumState> InitialiseImperiumState(IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var state = services.GetRequiredService<ImperiumState>();
        var statusService = services.GetRequiredService<IStatusService>();
        var imperiumDirectories = services.GetRequiredService<ImperiumDirectories>();

        var deviceControllerFactory = services.GetRequiredService<IDeviceControllerFactory>();

        state.AddMekatrolDeviceControllers(services);
        state.AddDeviceController(ImperiumConstants.VirtualKey, new VirtualPointDeviceController());
        state.AddDeviceController(ImperiumConstants.MqttKey, new MqttPointDeviceController(services));

        // Get all device files
        var deviceFiles = Directory.GetFiles(imperiumDirectories.Devices, "*.json");

        foreach (var deviceFile in deviceFiles)
        {
            var correlationId = statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Information, deviceFile, $"Starting device initialisation.");

            statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Debug, deviceFile, "Loading configuration file.", correlationId);
            var json = await File.ReadAllTextAsync(deviceFile, cancellationToken);

            statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Debug, deviceFile, "Deserializing JSON.", correlationId);
            var config = JsonSerializer.Deserialize<DeviceConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions)!;

            try
            {
                Assembly? assembly = null;

                if (!string.IsNullOrWhiteSpace(config.JsonTransformScriptFile))
                {
                    statusService.ReportItem(
                        KnownStatusCategories.Configuration,
                        StatusItemSeverity.Debug,
                        deviceFile,
                        $"Compiling device script: '{config.JsonTransformScriptFile}'.",
                        correlationId);

                    var assemblyName = "Device_" + Path.GetFileNameWithoutExtension(config.JsonTransformScriptFile).Replace(".", "_");

                    assembly = await ScriptHelper.CompileJsonTransformerScript(
                        services,
                        assemblyName,
                        imperiumDirectories.Scripts,
                        config.JsonTransformScriptFile,
                        correlationId,
                        cancellationToken);
                }

                // Only add if there are no scripts or no script errors
                if (string.IsNullOrWhiteSpace(config.JsonTransformScriptFile) || assembly != null)
                {
                    deviceControllerFactory.AddDeviceInstance(
                        config.DeviceKey,
                        config.ControllerKey,
                        config.Data,
                        config.Points,
                        state,
                        assembly);

                    statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Information, config.DeviceKey, $"Device initialisation success.", correlationId);
                }
                else
                {
                    statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Warning, config.DeviceKey, "Device initialisation failed.", correlationId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex);

                statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Error, config.DeviceKey, ex.ToString(), correlationId);
                statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Error, config.DeviceKey, "Device initialisation failed.", correlationId);
            }
        }

        return state;
    }
}
