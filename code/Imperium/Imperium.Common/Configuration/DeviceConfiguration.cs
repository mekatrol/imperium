using Imperium.Common.Points;

namespace Imperium.Common.Configuration;

public class DeviceConfiguration
{
    /// <summary>
    /// The key identifying which controller is used to communication with this device.
    /// </summary>
    public string ControllerKey { get; set; } = string.Empty;

    /// <summary>
    /// The unique key identifying this device instance.
    /// </summary>
    public string DeviceKey { get; set; } = string.Empty;

    /// <summary>
    /// Will be set to true if the device instance is currently enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The duration after which the device has an offline status if it has not been commuicated with.
    /// </summary>
    public TimeSpan OfflineStatusDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The mapped points for this device.
    /// </summary>
    public IList<PointDefinition> Points { get; set; } = [];

    /// <summary>
    /// A set of JSON data specific to this instance. The actuall JSON structure is
    /// specific to the type of device.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// The filename of the script that allows mapping between message data and point data
    /// </summary>
    public string? JsonTransformScriptFile { get; set; }
}
