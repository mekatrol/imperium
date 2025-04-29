using Imperium.Server.Options;
using Imperium.Server.Services;

namespace Imperium.Server.Background;

internal class TimerBackgroundService(
    TimerBackgroundServiceOptions backgroundServiceOptions,
    IServiceProvider serviceProvider,
    ILogger<TimerBackgroundService> logger)
    : BaseBackgroundService<TimerBackgroundService>(
        backgroundServiceOptions,
        serviceProvider,
        logger)
{
    private DateTime _lastTickDateTime = DateTime.Now;

    protected override Task<bool> ExecuteIteration(IServiceProvider services, CancellationToken stoppingToken)
    {
        var now = DateTime.Now;

        // Are we in the same hour as last tick?
        if (_lastTickDateTime.Hour != now.Hour)
        {
            _lastTickDateTime = now;

            // This will force a reload of the SPA dashboard UI
            var appVersionService = services.GetRequiredService<IAppVersionService>();
            appVersionService.ExecutionVersion = Guid.NewGuid();
        }

        return Task.FromResult(true);
    }
}
