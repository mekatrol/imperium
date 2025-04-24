namespace Imperium.Common.Points;

public class PointUpdateValueModel
{
    public string DeviceKey { get; set; }

    public string PointKey { get; set; } = string.Empty;

    /// <summary>
    /// The action to perform
    /// </summary>
    public PointUpdateAction PointUpdateAction { get; set; } = PointUpdateAction.Control;

    /// <summary>
    /// The new value as a string, it must be convertable to a point type value.
    /// </summary>
    public string? Value { get; set; }
}
