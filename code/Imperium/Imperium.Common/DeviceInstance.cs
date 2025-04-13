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

    public Point CreatePoint<T>(string key, string friendlyName, object value) where T : struct
    {
        // Get with default value
        var point = GetPointWithDefault<T>(key);

        // Set friendly name
        point.FriendlyName = friendlyName;

        // Set initial value
        point.Value = value;

        // Return created point
        return point;
    }

    public Point GetPointWithDefault<T>(string pointKey, Point? defaultValue = null) where T : struct
    {
        // Try and get existing by key
        var point = _points.SingleOrDefault(p => p.Key == pointKey);

        // If found then return it
        if (point != null)
        {
            return point;
        }

        var pointType = GetPointType(typeof(T)) ?? throw new InvalidOperationException($"The type of point '{typeof(T).FullName}' is not valid for the type.");

        // Not found then use default or create new
        point = defaultValue ?? new Point(pointType) { Key = pointKey, FriendlyName = pointKey, DeviceKey = Key };

        // Add the new point
        _points.Add(point);

        // Return the point
        return point;
    }

    public override string ToString()
    {
        return $"{{ Key='{Key}', ControllerKey='{ControllerKey}' }}";
    }

    public static PointType? GetPointType(Type type)
    {
        if (type == typeof(int)) return PointType.Integer;
        if (type == typeof(float)) return PointType.Floating;
        if (type == typeof(double)) return PointType.Floating;
        if (type.IsEnum) return PointType.Enum;

        return null;
    }
}
