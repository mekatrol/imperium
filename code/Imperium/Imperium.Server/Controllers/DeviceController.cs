using Imperium.Common.Models;
using Imperium.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imperium.Server.Controllers;

[ApiController]
[Route("api/devices")]
public class DeviceController(ILogger<DeviceController> logger, IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public async Task<IList<Device>> Get(CancellationToken cancellationToken)
    {
        logger.LogDebug("{msg}", $"Getting all devices.");
        var deviceService = services.GetRequiredService<IDeviceService>();
        return await deviceService.GetAllDevices(cancellationToken);
    }
}
