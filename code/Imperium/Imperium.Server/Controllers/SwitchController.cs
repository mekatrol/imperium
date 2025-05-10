using Imperium.Common.Models;
using Imperium.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/switches")]
public class SwitchController(ILogger<SwitchController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public IList<SwitchEntity> Get([FromQuery] IList<string>? keys = null)
    {
        logger.LogDebug("{msg}",
            keys == null || keys.Count == 0
            ? "Getting all switches."
            : $"Getting switches {string.Join(',', keys)}.");

        var switchService = services.GetRequiredService<ISwitchService>();
        return switchService.GetSwitches(keys);
    }
}
