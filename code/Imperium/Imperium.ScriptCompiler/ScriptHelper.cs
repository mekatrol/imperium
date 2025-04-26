using Imperium.Common.Extensions;
using Imperium.Common.Scripting;
using Imperium.Common.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Imperium.ScriptCompiler;

public class ScriptHelper
{
    public static async Task<bool> CompileJsonTransformerScript(
        IServiceProvider services,
        string scriptFileDirectory,
        string scriptFileName,
        Guid correlationId)
    {
        var logger = services.GetRequiredService<ILogger<ScriptHelper>>();
        var statusService = services.GetRequiredService<IStatusService>();

        try
        {
            var scriptFullpath = Path.Combine(scriptFileDirectory, scriptFileName);

            var code = await File.ReadAllTextAsync(scriptFullpath);

            var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var executingAssemblyPath = Path.GetFullPath(currentAssemblyDirectory);

            IList<string> additionalAssemblies = ["System.Runtime.dll", "System.Private.CoreLib.dll", "System.Text.Json.dll", "Imperium.Common.dll"];

            var scriptError = false;

            // Try and load compiler and assembly
            var (context, assembly, errors) = ScriptAssemblyContext.LoadAndCompile(
                executingAssemblyPath,
                code,
                additionalAssemblies,
                () => { /* unload */ });

            if (errors.Count > 0)
            {
                scriptError = true;

                foreach (var error in errors)
                {
                    statusService.ReportItem(KnownStatusCategories.Scripting, StatusItemSeverity.Error, scriptFileName, error, correlationId);
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
                if (isAssignable == 0)
                {
                    statusService.ReportItem(
                        KnownStatusCategories.Scripting,
                        StatusItemSeverity.Error,
                        scriptFileName,
                        $"There was no class found that implements '{nameof(IJsonMessageTransformer)}'.",
                        correlationId);

                    scriptError = true;
                }
                else if (isAssignable > 1)
                {
                    statusService.ReportItem(
                        KnownStatusCategories.Scripting,
                        StatusItemSeverity.Error,
                        scriptFileName,
                        $"There are multiple classes that implement '{nameof(IJsonMessageTransformer)}'. There should only be one.",
                        correlationId);

                    scriptError = true;
                }
                else
                {
                    statusService.ReportItem(
                        KnownStatusCategories.Scripting,
                        StatusItemSeverity.Information,
                        scriptFileName,
                        $"Compilation success.",
                        correlationId);
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

            return scriptError;
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
            statusService.ReportItem(
                KnownStatusCategories.Scripting,
                StatusItemSeverity.Error,
                scriptFileName,
                ex.ToString(),
                correlationId);

            return false;
        }
    }
}
