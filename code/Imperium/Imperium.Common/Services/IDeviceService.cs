using Imperium.Common.Models;

namespace Imperium.Common.Services;

public interface IDeviceService
{
    Task<IList<Device>> GetAllDevices(CancellationToken cancellationToken = default);
}
