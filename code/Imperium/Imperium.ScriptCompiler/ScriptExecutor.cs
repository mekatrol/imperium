using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Imperium.ScriptCompiler;

public class ScriptExecutor
{
    public static async Task<IList<string>> RunAndUnload(
        string assemblyName,
        string executingAssemblyPath,
        string sourceCode,
        IList<string> additionalAssemblies,
        Func<Assembly, CancellationToken, Task> executeScript,
        Action unload,
        int unloadMaxAttempts,
        int unloadDelayBetweenTries,
        CancellationToken stoppingToken = default)
    {
        var (weakRef, errors) = await Run(assemblyName, executingAssemblyPath, sourceCode, additionalAssemblies, executeScript, unload, stoppingToken);

        if (weakRef != null)
        {
            await WaitForUnload(weakRef, unloadMaxAttempts, unloadDelayBetweenTries, stoppingToken);
        }

        return errors;
    }

    public static (ScriptAssemblyContext, Assembly?, IList<string>) Load(
        string assemblyName,
        string executingAssemblyPath,
        string sourceCode,
        IList<string> additionalAssemblies,
        Action unload)
    {
        var (scriptCompiler, assembly, errors) = ScriptAssemblyContext.LoadAndCompile(
            assemblyName,
            executingAssemblyPath,
            sourceCode,
            additionalAssemblies,
            unload,
            OptimizationLevel.Debug);

        return (scriptCompiler, assembly, errors);
    }

    public static async Task<(WeakReference?, IList<string>)> Run(
        string assemblyName,
        string executingAssemblyPath,
        string sourceCode,
        IList<string> additionalAssemmblies,
        Func<Assembly, CancellationToken, Task> executeScript,
        Action unload,
        CancellationToken stoppingToken = default)
    {
        // Try and load compiler and assembly
        var (scriptCompiler, assembly, errors) = Load(assemblyName, executingAssemblyPath, sourceCode, additionalAssemmblies, unload);

        // Create a weak reference to the AssemblyLoadContext that will allow us to detect
        // when the unload completes.
        var alcWeakRef = new WeakReference(scriptCompiler);

        if (assembly == null || errors.Count > 0)
        {
            return (alcWeakRef, errors);
        }

        await executeScript(assembly, stoppingToken);

        scriptCompiler.Unload();

        return (alcWeakRef, new List<string>());
    }

    public static async Task WaitForUnload(
        WeakReference weakRef,
        int maxAttempts,
        int delayBetweenTries,
        CancellationToken stoppingToken)
    {
        for (var i = 0; weakRef.IsAlive && (i < maxAttempts); i++)
        {
            await Task.Delay(delayBetweenTries, stoppingToken);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

    }
}
