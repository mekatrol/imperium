using Imperium.Common.Points;

namespace Imperium.Common.Extensions;

public static class PointExtensions
{
    public static PointType? GetPointType(this Type type)
    {
        if (type == typeof(int)) return PointType.Integer;
        if (type == typeof(float)) return PointType.SingleFloat;
        if (type == typeof(double)) return PointType.DoubleFloat;
        if (type == typeof(bool)) return PointType.Boolean;
        if (type == typeof(string)) return PointType.String;
        if (type == typeof(DateTime)) return PointType.DateTime;
        if (type == typeof(DateOnly)) return PointType.DateOnly;
        if (type == typeof(TimeOnly)) return PointType.TimeOnly;

        return null;
    }
}
