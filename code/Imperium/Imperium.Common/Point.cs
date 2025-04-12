namespace Imperium.Common;

public abstract class Point(PointType pointType)
{
    public string Id { get; set; } = string.Empty;

    public PointType PointType { get; set; } = pointType;
}
