using Imperium.Common;
using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Directories;
using Imperium.Common.Extensions;
using Imperium.Common.Models;
using Imperium.Common.Points;
using Imperium.Common.Status;
using Imperium.Common.Utils;
using Imperium.ScriptCompiler;
using Imperium.Server.DeviceControllers;
using Imperium.Server.Models;
using System.Reflection;
using System.Text.Json;

namespace Imperium.Server.State;

public class StatePersistor
{
    public const string DeviceControllersConfigurationFilename = "device-controllers.json";
    public const string MqttConfigurationFilename = "mqtt.json";

    public static async Task<IImperiumState> LoadState(IServiceProvider services, CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var state = services.GetRequiredService<IImperiumState>();
        var statusService = services.GetRequiredService<IStatusService>();
        var imperiumDirectories = services.GetRequiredService<ImperiumDirectories>();

        var statusReporter = statusService.CreateStatusReporter(KnownStatusCategories.Configuration, nameof(StatePersistor));

        // Add known device controllers
        state.AddDeviceController(ImperiumConstants.VirtualKey, new VirtualPointDeviceController());
        state.AddDeviceController(ImperiumConstants.MqttKey, new MqttPointDeviceController(services));

        // Load configured device controllers
        var deviceControllerConfigurationFilename = Path.Combine(imperiumDirectories.Base, DeviceControllersConfigurationFilename);
        if (File.Exists(deviceControllerConfigurationFilename))
        {
            var json = await File.ReadAllTextAsync(deviceControllerConfigurationFilename, cancellationToken);
            var deviceControllerConfiguration = JsonSerializer.Deserialize<List<DeviceControllerFactoryConfiguration>>(json, JsonSerializerExtensions.DefaultSerializerOptions);

            if (deviceControllerConfiguration == null)
            {
                statusReporter.ReportItem(StatusItemSeverity.Error, $"Failed to deserialise file '{deviceControllerConfigurationFilename}'. Please ensure the file is valid JSON.");
            }
            else
            {
                foreach (var dc in deviceControllerConfiguration)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(dc.Library))
                        {
                            var libraryPath = dc.Library;

                            // If the path is relative, then it is relative to the configuration file folder and we convert to absolute path
                            if (!Path.IsPathRooted(libraryPath))
                            {
                                libraryPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(deviceControllerConfigurationFilename)!, libraryPath));
                            }

                            // A library has been defined so try and load it
                            var assembly = Assembly.LoadFrom(libraryPath);

                            // Now get the configured type 
                            var type = assembly.GetType(dc.Factory);

                            if (type == null)
                            {
                                statusReporter.ReportItem(
                                    StatusItemSeverity.Error,
                                    $"The device controller factory type '{dc.Factory}' could not be loaded from the library '{libraryPath}'.");

                                continue;
                            }

                            if (type != null)
                            {
                                // The type must be a device controller factory derived non-abstract class
                                if (!typeof(IDeviceControllerFactory).IsAssignableFrom(type) ||
                                    type.IsInterface ||
                                    type.IsAbstract)
                                {
                                    statusReporter.ReportItem(
                                        StatusItemSeverity.Error,
                                        $"The device controller factory type '{dc.Factory}' initialisation failed.");

                                    continue;
                                }

                                // Create an instance
                                var instance = Activator.CreateInstance(type);

                                var interfaceType = typeof(IDeviceControllerFactory);
                                var interfaceMethod = interfaceType.GetMethod(nameof(IDeviceControllerFactory.AddDeviceController))!;

                                var paramTypes = new List<Type>();
                                var parameters = interfaceMethod.GetParameters();
                                foreach (var param in parameters)
                                {
                                    //Console.WriteLine($"- {param.ParameterType.Name} {param.Name}");
                                    paramTypes.Add(param.ParameterType);
                                }

                                // Get the method that matches the following param signature                                
                                var method = type.GetMethod(nameof(IDeviceControllerFactory.AddDeviceController), paramTypes.ToArray());

                                if (method == null || method.ReturnType != interfaceMethod.ReturnType)
                                {
                                    var message = $"Type '{type.FullName}' does not implement the method '{interfaceMethod.ReturnType.GetTypeAlias()} {interfaceMethod.Name}({string.Join(", ", parameters.Select(p => $"{p.ParameterType.GetTypeAlias()} {p.Name}"))})'.";
                                    statusReporter.ReportItem(StatusItemSeverity.Error, message);
                                    continue;
                                }

                                // Invoke the method
                                var controller = method.Invoke(instance, [services, dc.Key]);

                                if (controller == null)
                                {
                                    var message = $"Type '{type.FullName}' does not implment the controller key '{dc.Key}'.";
                                    statusReporter.ReportItem(StatusItemSeverity.Error, message);
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        statusReporter.ReportItem(StatusItemSeverity.Error, ex);
                        statusReporter.ReportItem(StatusItemSeverity.Error, $"Device controller factory type '{dc.Factory}' initialisation failed.");
                    }
                }
            }
        }

        //state.AddMekatrolDeviceControllers(services);

        // Get all device files
        var deviceFiles = Directory.GetFiles(imperiumDirectories.Devices, "*.json");

        var deviceInstanceFactory = services.GetRequiredService<IDeviceInstanceFactory>();
        foreach (var deviceFile in deviceFiles)
        {
            var correlationId = statusService.ReportItem(KnownStatusCategories.Configuration, StatusItemSeverity.Information, deviceFile, $"Starting device initialisation.");

            statusReporter.ReportItem(StatusItemSeverity.Debug, $"Loading configuration file '{deviceFile}'.");
            var json = await File.ReadAllTextAsync(deviceFile, cancellationToken);

            statusReporter.ReportItem(StatusItemSeverity.Debug, $"Deserializing JSON from '{deviceFile}'.");
            var config = JsonSerializer.Deserialize<DeviceConfiguration>(json, JsonSerializerExtensions.DefaultSerializerOptions)!;

            try
            {
                Assembly? assembly = null;

                if (!string.IsNullOrWhiteSpace(config.JsonTransformScriptFile))
                {
                    statusReporter.ReportItem(
                        StatusItemSeverity.Debug,
                        $"Compiling device script: '{config.JsonTransformScriptFile}' reference by device file '{deviceFile}'.");

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
                    var deviceInstance = deviceInstanceFactory.AddDeviceInstance(
                        config.DeviceKey,
                        config.ControllerKey,
                        config.ControllerKey == ImperiumConstants.VirtualKey ? DeviceType.Virtual : DeviceType.Physical,
                        config.Data,
                        config.Points,
                        state,
                        assembly);

                    deviceInstance.OfflineStatusDuration = config.OfflineStatusDuration;

                    statusReporter.ReportItem(StatusItemSeverity.Information, $"Device with key '{config.DeviceKey}' initialisation succeeded.");
                }
                else
                {
                    statusReporter.ReportItem(StatusItemSeverity.Warning, $"Device with key '{config.DeviceKey}' initialisation failed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex);

                statusReporter.ReportItem(StatusItemSeverity.Error, ex);
                statusReporter.ReportItem(StatusItemSeverity.Error, $"Device with key '{config.DeviceKey}' initialisation failed.");
            }
        }

        return state;
    }
}
