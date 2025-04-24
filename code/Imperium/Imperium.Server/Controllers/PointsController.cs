using Imperium.Common.Points;
using Imperium.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/points")]
public class PointsController(ILogger<PointsController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public IList<Point> Get()
    {
        logger.LogDebug("{msg}", $"Getting all points.");
        var pointService = services.GetRequiredService<IPointService>();
        return pointService.GetAllPoints();
    }

    [HttpPost]
    public async Task<Point?> Post(PointUpdateValueModel pointUpdate)
    {
        logger.LogDebug("{msg}", $"Updating point '{pointUpdate}'");
        var pointService = services.GetRequiredService<IPointService>();

        return await pointService.UpdatePoint(pointUpdate);
    }
}
