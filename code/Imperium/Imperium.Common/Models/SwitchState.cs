namespace Imperium.Common.Models;

[Flags]
public enum SwitchState
{
    Offline = 2 ^ 0,
    Off = 2 ^ 1,
    On = 2 ^ 2,
    Disabled = 2 ^ 3,
    Overridden = 2 ^ 4
}
