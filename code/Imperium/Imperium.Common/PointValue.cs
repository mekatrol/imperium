namespace Imperium.Common;

public class PointValue<T>() : Point(GetPointType(typeof(T))) where T : struct
{
    public T Value
    {
        get
        {
            return (T) Current!;
        }
        set
        {
            Current = value;
        }
    }

    public override string ToString()
    {
        return $"Id={ Id }, Value = { Value }, Type = { PointType }";
    }

    private static PointType GetPointType(Type type) 
    {
        if (type == typeof(int)) return PointType.Integer;
        if (type == typeof(float)) return PointType.Floating;
        if (type == typeof(double)) return PointType.Floating;
        if (type.IsEnum) return PointType.Enum;

        throw new Exception($"Unknown value type '{type.FullName}'");
    }
}
