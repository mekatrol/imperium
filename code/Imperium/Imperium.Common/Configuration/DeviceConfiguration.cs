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
    /// The mapped points for this device.
    /// </summary>
    public IList<PointDefinition> Points { get; set; } = [];

    /// <summary>
    /// A set of JSON data specific to this instance. The actuall JSON structure is
    /// specific to the type of device.
    /// </summary>
    public string Data {  get; set; } = string.Empty;
}
