namespace Imperium.Common.Extensions;

public static class PointExtensions
{
    public static PointType? GetPointType(this Type type)
    {
        if (type == typeof(int)) return PointType.Integer;
        if (type == typeof(float)) return PointType.Floating;
        if (type == typeof(double)) return PointType.Floating;
        if (type == typeof(bool)) return PointType.Boolean;
        if (type.IsEnum) return PointType.Enum;

        return null;
    }
}
