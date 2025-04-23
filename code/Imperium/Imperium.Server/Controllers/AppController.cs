using Imperium.Common.Configuration;
using Imperium.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/app")]
public class AppController(ILogger<AppController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet("version")]
    public AppVersionConfiguration Get()
    {
        var appVersionService = services.GetRequiredService<IAppVersionService>();

        logger.LogDebug("{msg}", $"Getting app version '{appVersionService.ExecutionVersion}'.");

        return new AppVersionConfiguration
        {
            ApplicationVersion = appVersionService.ApplicationVersion.ToString(),
            ExecutionVersion = appVersionService.ExecutionVersion.ToString(),
        };
    }
}
