using Imperium.Common.Extensions;
using Imperium.Common.Status;
using Imperium.Server.Options;

namespace Imperium.Server.Background;

internal abstract class BaseBackgroundService<T> : BackgroundService
{
    protected readonly ILogger Logger;
    protected readonly IServiceProvider Services;
    protected readonly IStatusService StatusService;
    protected readonly IStatusReporter StatusReporter;

    private readonly BackgroundServiceOptions _backgroundServiceOptions;

    protected BaseBackgroundService(
        BackgroundServiceOptions backgroundServiceOptions,
        IServiceProvider serviceProvider,
        ILogger<T> logger)
    {
        _backgroundServiceOptions = backgroundServiceOptions;

        Logger = logger;
        Services = serviceProvider;
        StatusService = serviceProvider.GetRequiredService<IStatusService>();
        StatusReporter = StatusService.CreateStatusReporter(KnownStatusCategories.BackgroundTask, typeof(T).Name);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{msg}", $"Starting {nameof(T)} background service");

        var exceptionCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                if (!await ExecuteIteration(services, stoppingToken))
                {
                    // Derived class asked for background service to exit
                    return;
                }

                await Task.Delay(_backgroundServiceOptions.LoopIterationSleep, stoppingToken);

                // No exceptions so reset exception count
                exceptionCount = 0;
            }
            catch (TaskCanceledException) { /* Server is shutting down, ignore this error */ }
            catch (Exception ex)
            {
                Logger.LogError(ex);

                if (++exceptionCount >= _backgroundServiceOptions.MaxConsecutiveExceptions)
                {
                    // If we have configured consecutive exceptions then we give up!
                    Logger.LogError("{msg}", $"Stopping {nameof(T)} due to too many consecutive exceptions.");
                    return;
                }

                // Sleep for configured number seconds to try and let things settle (esp if exception keeps occuring)
                await Task.Delay(_backgroundServiceOptions.LoopExceptionSleep, stoppingToken);
            }
        }

        Logger.LogDebug("{msg}", $"Exiting {nameof(T)} background service");
    }

    protected abstract Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken);
}
