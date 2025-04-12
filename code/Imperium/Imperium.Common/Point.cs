namespace Imperium.Common;

public abstract class Point(PointType pointType)
{
    public string Id { get; set; } = string.Empty;

    public PointType PointType { get; set; } = pointType;

    public object? Current { get; set; }

    public string? FriendlyName { get; set; }
}
