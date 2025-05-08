using Imperium.Common.Models;
using Imperium.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/dashboards")]
public class DashboardController(ILogger<DeviceController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public IList<Dashboard> Get()
    {
        logger.LogDebug("{msg}", $"Getting all dashboards.");
        var dashboardService = services.GetRequiredService<IDashboardService>();
        return dashboardService.GetAllDashboards();
    }

    [HttpGet("{name}")]
    public Dashboard Get([FromRoute] string name)
    {
        logger.LogDebug("{msg}", $"Getting dashboard '{name}'.");
        var dashboardService = services.GetRequiredService<IDashboardService>();
        return dashboardService.GetDashboard(name);
    }
}
