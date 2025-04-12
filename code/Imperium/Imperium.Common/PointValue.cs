namespace Imperium.Common;

public class PointValue<T>(PointType type) : Point(type) where T : struct
{
    public T Value { get; set; }

    public override string ToString()
    {
        return $"Id={ Id }, Value = { Value }, Type = { PointType }";
    }
}
