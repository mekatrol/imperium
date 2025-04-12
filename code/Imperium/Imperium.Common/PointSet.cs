namespace Imperium.Common;

public class PointSet(string key)
{
    private readonly IList<Point> _points = [];

    public string Key { get; } = key;

    public IList<Point> Points
    {
        get
        {
            // Return a copy so that it cannot be modified
            return [.. _points];
        }
    }

    public T CreatePoint<T>(string id, string friendlyName, object value) where T : Point, new()
    {
        // Get with default value
        var point = GetPointWithDefault<T>(id);

        // Set friendly name
        point.FriendlyName = friendlyName;

        // Set initial value
        point.Current = value;

        // Return created point
        return point;
    }

    public T GetPointWithDefault<T>(string id, T? defaultValue = null) where T : Point, new()
    {
        // Try and get existing by ID
        var point = (T?)_points.SingleOrDefault(p => p.Id == id);

        // If found then return it
        if (point != null)
        {
            return point;
        }

        // Not found then use default or create new
        point = defaultValue ?? new T() { Id = id, FriendlyName = id };

        // Add the new point
        _points.Add(point);

        // Return the point
        return point;
    }
}
