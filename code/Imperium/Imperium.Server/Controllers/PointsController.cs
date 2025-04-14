using Imperium.Common.Points;
using Imperium.Server.State;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("points")]
public class PointsController(ILogger<PointsController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public IList<Point> Get()
    {
        logger.LogDebug("{msg}", $"Getting all points.");
        var state = services.GetRequiredService<ImperiumState>();
        return state.GetAllPoints();
    }
}
