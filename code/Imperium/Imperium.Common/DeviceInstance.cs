namespace Imperium.Common;

public class DeviceInstance(string key, string deviceControllerKey, bool enabled = true) : IDeviceInstance
{
    private readonly IList<Point> _points = [];

    public string Key { get; } = key;

    public string ControllerKey { get; } = deviceControllerKey;

    public bool Enabled { get; set; } = enabled;

    public IList<Point> Points
    {
        get
        {
            // Return a copy so that it cannot be modified
            return [.. _points];
        }
    }

    public T CreatePoint<T>(string key, string friendlyName, object value) where T : Point, new()
    {
        // Get with default value
        var point = GetPointWithDefault<T>(key);

        // Set friendly name
        point.FriendlyName = friendlyName;

        // Set initial value
        point.Current = value;

        // Return created point
        return point;
    }

    public T GetPointWithDefault<T>(string key, T? defaultValue = null) where T : Point, new()
    {
        // Try and get existing by key
        var point = (T?)_points.SingleOrDefault(p => p.Key == key);

        // If found then return it
        if (point != null)
        {
            return point;
        }

        // Not found then use default or create new
        point = defaultValue ?? new T() { Key = key, FriendlyName = key, DeviceKey = this.Key };

        // Add the new point
        _points.Add(point);

        // Return the point
        return point;
    }

    public override string ToString()
    {
        return $"{{ Key='{Key}', ControllerKey='{ControllerKey}' }}";
    }
}
