namespace Imperium.Common;

public class PointValue<T>(PointType type) : Point(type) where T : struct
{
    public T Value
    {
        get
        {
            return (T) V;
        }
        set
        {
            V = value;
        }
    }

    public override string ToString()
    {
        return $"Id={ Id }, Value = { Value }, Type = { PointType }";
    }
}
