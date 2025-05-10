using Imperium.Common.Configuration;

namespace Imperium.Common.Models;

public class SwitchEntity : SwitchBase
{
    /// <summary>
    /// The current value for the switch:
    /// null  -> the switch is offline (the value has not yet been set after system boot)
    /// false -> the switch is off
    /// true  -> the switch is on
    /// </summary>
    public SwitchState State { get; set; } = SwitchState.Offline;

    /// <summary>
    /// If not null then this is the date/time that the switch
    /// is scheduled to toggle from its current state either:
    /// off -> on
    /// on  -> off
    /// </summary>
    public DateTime? ToggleSchedule { get; set; }
}
