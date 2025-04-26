using Imperium.Common.Status;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController(IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public IList<IStatusItem> Get([FromQuery] IList<StatusItemSeverity>? withSeverities = null)
    {
        var statusService = services.GetRequiredService<IStatusService>();
        return statusService.GetStatuses(withSeverities);
    }

    [HttpGet("{correlationId}")]
    public IList<IStatusItem> Get(Guid correlationId)
    {
        var statusService = services.GetRequiredService<IStatusService>();
        return statusService.GetCorrelationStatuses(correlationId);
    }

    [HttpDelete]
    public void Delete([FromQuery] DateTime? olderThan)
    {
        var statusService = services.GetRequiredService<IStatusService>();
        statusService.ClearOlderThan(olderThan);
    }
}
