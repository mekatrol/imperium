using Imperium.Common.Extensions;
using Imperium.Server.Options;

namespace Imperium.Server.Background;

internal abstract class BaseBackgroundService<T>(
    BackgroundServiceOptions backgroundServiceOptions,
    IServiceProvider serviceProvider,
    ILogger<T> logger) : BackgroundService()
{
    protected ILogger Logger = logger;
    protected IServiceProvider Services = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{msg}", $"Starting {nameof(T)} background service");

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

                await Task.Delay(backgroundServiceOptions.LoopIterationSleep, stoppingToken);

                // No exceptions so reset exception count
                exceptionCount = 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex);

                if (++exceptionCount >= backgroundServiceOptions.MaxConsecutiveExceptions)
                {
                    // If we have configured consecutive exceptions then we give up!
                    logger.LogError("{msg}", $"Stopping {nameof(T)} due to too many consecutive exceptions.");
                    return;
                }

                // Sleep for configured number seconds to try and let things settle (esp if exception keeps occuring)
                await Task.Delay(backgroundServiceOptions.LoopExceptionSleep, stoppingToken);
            }
        }

        logger.LogDebug("{msg}", $"Exiting {nameof(T)} background service");
    }

    protected abstract Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken);
}
