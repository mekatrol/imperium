using Imperium.Common.Exceptions;
using Imperium.Common.Points;
using Imperium.Server.State;
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
        var state = services.GetRequiredService<ImperiumState>();
        return state.GetAllPoints();
    }

    [HttpPost]
    public Point? Post(PointUpdateValueModel pointUpdate)
    {
        logger.LogDebug("{msg}", $"Updating point '{pointUpdate}'");

        var pointState = services.GetRequiredService<IPointState>();

        var updatedPoint = pointState.UpdatePointValue(pointUpdate.Id, pointUpdate.Value, PointValueType.Override);

        if (updatedPoint == null)
        {
            throw new NotFoundException($"A point with the ID '{pointUpdate.Id}' was not found.", nameof(PointUpdateValueModel.Id));
        }

        return updatedPoint;
    }
}
