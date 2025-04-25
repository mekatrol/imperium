using Imperium.Common;
using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Extensions;
using Imperium.Common.Points;
using Imperium.Common.Scripting;
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
        var scriptDirectory = Path.Combine(configurationPath, "scripts");

        Directory.CreateDirectory(devicesDirectory);
        Directory.CreateDirectory(pointsDirectory);
        Directory.CreateDirectory(scriptDirectory);

        var deviceControllerFactory = services.GetRequiredService<IDeviceControllerFactory>();

        state.AddMekatrolDeviceControllers(services);
        state.AddDeviceController(ImperiumConstants.VirtualKey, new VirtualPointDeviceController());
        state.AddDeviceController(ImperiumConstants.MqttKey, new MqttPointDeviceController());

        // Get all device files
        var deviceFiles = Directory.GetFiles(devicesDirectory, "*.json");

        foreach (var deviceFile in deviceFiles)
        {
            var json = await File.ReadAllTextAsync(deviceFile);
            var config = JsonSerializer.Deserialize<DeviceConfiguration>(json, JsonSerializerExtensions.ApiSerializerOptions)!;

            try
            {
                deviceControllerFactory.AddDeviceInstance(
                    config.DeviceKey,
                    config.ControllerKey,
                    config.Data,
                    config.Points,
                    state);

                if (!string.IsNullOrWhiteSpace(config.JsonTransformScriptFile))
                {
                    try
                    {
                        var scriptFullpath = Path.Combine(scriptDirectory, config.JsonTransformScriptFile);

                        var code = await File.ReadAllTextAsync(scriptFullpath);

                        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                        var executingAssemblyPath = Path.GetFullPath(currentAssemblyDirectory);

                        IList<string> additionalAssemblies = ["System.Runtime.dll", "System.Private.CoreLib.dll", "System.Text.Json.dll", "Imperium.Common.dll"];

                        // Try and load compiler and assembly
                        var (context, assembly, errors) = ScriptAssemblyContext.LoadAndCompile(
                            executingAssemblyPath,
                            code,
                            additionalAssemblies,
                            () => { /* unload */ });

                        if (errors.Count > 0)
                        {
                            foreach (var error in errors)
                            {
                                Console.WriteLine(error);
                            }
                        }
                        else
                        {
                            var isAssignable = 0;

                            foreach (var definedType in assembly!.DefinedTypes)
                            {
                                if (definedType.IsAssignableTo(typeof(IJsonMessageTransformer)))
                                {
                                    isAssignable++;
                                }
                            }

                            // There should be exactly 1 type assignable from IJsonMessageTransformer
                            if (isAssignable != 1)
                            {
                                Console.WriteLine("Cannot assign");
                            }
                        }

                        // Unload loaded context
                        context.Unload();

                        //var compileErrors = await ScriptExecutor.RunAndUnload(
                        //    executingAssemblyPath,
                        //    code,
                        //    additionalAssemblies: ["System.Runtime.dll", "System.Private.CoreLib.dll", "System.Text.Json.dll", "Imperium.Common.dll"],
                        //    executeScript: async (assembly, stoppingToken) =>
                        //    {
                        //        // Get the plugin interface by calling the PluginClass.GetInterface method via reflection.
                        //        var scriptType = assembly.GetType("HouseAlarmTransformer") ?? throw new Exception("HouseAlarmTransformer");

                        //        var instance = Activator.CreateInstance(scriptType, true);

                        //        // Call script if not null an no errors
                        //        if (instance != null)
                        //        {
                        //            var execute = scriptType.GetMethod("FromDeviceJson", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("FromDeviceJson");

                        //            // Now we can call methods of the plugin using the interface
                        //            var executor = (Task<string>?)execute.Invoke(instance, ["{ \"zone\": 1, \"event\": \"EVENT\"  }", stoppingToken]);
                        //            var json = await executor!;

                        //            Console.WriteLine(json);
                        //        }
                        //    },
                        //    () =>
                        //    {
                        //        Console.WriteLine("Script assembly unloaded");
                        //    },
                        //    unloadMaxAttempts: 10, unloadDelayBetweenTries: 100, stoppingToken: CancellationToken.None);

                        //if (compileErrors.Count > 0)
                        //{
                        //    foreach (var error in compileErrors)
                        //    {
                        //        Console.WriteLine(error);
                        //    }
                        //}
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex);
            }
        }

        return state;
    }
}
