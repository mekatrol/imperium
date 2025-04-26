using Imperium.Common.Extensions;
using Imperium.Common.Scripting;
using Imperium.Common.Status;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Imperium.ScriptCompiler;

public class ScriptHelper
{
    public static async Task<Assembly?> CompileJsonTransformerScript(
        IServiceProvider services,
        string assemblyName,
        string scriptFileDirectory,
        string scriptFileName,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var logger = services.GetRequiredService<ILogger<ScriptHelper>>();
        var statusService = services.GetRequiredService<IStatusService>();

        try
        {
            var scriptFullpath = Path.Combine(scriptFileDirectory, scriptFileName);

            var code = await File.ReadAllTextAsync(scriptFullpath, cancellationToken);

            var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var executingAssemblyPath = Path.GetFullPath(currentAssemblyDirectory);

            IList<string> additionalAssemblies = [];

            // Try and load compiler and assembly
            var (context, assembly, errors) = ScriptAssemblyContext.LoadAndCompile(
                assemblyName,
                executingAssemblyPath,
                code,
                additionalAssemblies,
                () => { /* unload */ });

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    statusService.ReportItem(KnownStatusCategories.Scripting, StatusItemSeverity.Error, scriptFileName, error, correlationId);
                }

                return null;
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

                    return null;
                }

                if (isAssignable > 1)
                {
                    statusService.ReportItem(
                        KnownStatusCategories.Scripting,
                        StatusItemSeverity.Error,
                        scriptFileName,
                        $"There are multiple classes that implement '{nameof(IJsonMessageTransformer)}'. There should only be one.",
                        correlationId);

                    return null;
                }

                statusService.ReportItem(
                    KnownStatusCategories.Scripting,
                    StatusItemSeverity.Information,
                    scriptFileName,
                    $"Compilation success.",
                    correlationId);
            }

            // Unload loaded context
            context.Unload();

            return assembly;
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

            return null;
        }
    }

    public static Task<string> ExecuteJsonTransformerFromDeviceJsonScript(
        IServiceProvider services,
        Assembly assembly,
        string json,
        CancellationToken stoppingToken)
    {
        return ExecuteJsonTransformerScript(services, assembly, nameof(IJsonMessageTransformer.FromDeviceJson), json, stoppingToken);
    }

    public static Task<string> ExecuteJsonTransformerToDeviceJsonScript(
        IServiceProvider services,
        Assembly assembly,
        string json,
        CancellationToken stoppingToken)
    {
        return ExecuteJsonTransformerScript(services, assembly, nameof(IJsonMessageTransformer.ToDeviceJson), json, stoppingToken);
    }

    private static Task<string> ExecuteJsonTransformerScript(
        IServiceProvider services,
        Assembly assembly,
        string methodName,
        string json,
        CancellationToken stoppingToken)
    {
        // There should be exactly one type that is assignable from IJsonMessageTransformer because that was
        // validated during compilation of the assembly
        var type = assembly.DefinedTypes.Single(t => t.IsAssignableTo(typeof(IJsonMessageTransformer)));

        var hasServicesConstructor = type.GetConstructors()
            .Any(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(IServiceProvider);
            });

        // Create an instance of the type
        object[] constructorArgs = [services];
        var instance = hasServicesConstructor
            ? Activator.CreateInstance(type, constructorArgs)
            : Activator.CreateInstance(type, true);

        if (instance == null)
        {
            throw new Exception($"Unable to create an instance of the type '{type.FullName}'.");
        }

        // Get the transform method
        var execute = type.GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public)!;

        // Execute the transform method
        return ((Task<string>?)execute.Invoke(instance, [json, stoppingToken]))!;
    }
}
