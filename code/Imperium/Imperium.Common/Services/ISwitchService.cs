using Imperium.Common.Models;

namespace Imperium.Common.Services;

public interface ISwitchService
{
    IList<SwitchEntity> GetSwitches(IList<string>? keys = null);
}
