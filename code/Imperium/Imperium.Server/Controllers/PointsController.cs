using Imperium.Common;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PointsController(ILogger<PointsController> logger, IServiceProvider services) : ControllerBase
{
    private readonly ILogger<PointsController> _logger = logger;

    [HttpGet(Name = "all")]
    public IEnumerable<PointSet> Get()
    {
        var allPoints = services.GetRequiredService<ConcurrentDictionary<string, PointSet>>();
        return allPoints.Values;
    }
}
