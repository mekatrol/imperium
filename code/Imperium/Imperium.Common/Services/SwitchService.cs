using Imperium.Common.Models;
using Imperium.Common.Points;
using Microsoft.Extensions.DependencyInjection;

namespace Imperium.Common.Services;

internal class SwitchService(IServiceProvider services) : ISwitchService
{
    public IList<SwitchEntity> GetSwitches(IList<string>? keys = null)
    {
        var state = services.GetRequiredService<IImperiumState>();
        return state.GetSwitches(keys);
    }
}
