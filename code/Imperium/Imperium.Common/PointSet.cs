namespace Imperium.Common;

public class PointSet(string key)
{
    public string Key { get; } = key;

    public IList<Point> Points { get; set; } = [];
}
