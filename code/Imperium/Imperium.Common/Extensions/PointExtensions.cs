using Imperium.Common.Points;

namespace Imperium.Common.Extensions;

public static class PointExtensions
{
    public static PointType? GetPointType(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(int))
        {
            return PointType.Integer;
        }

        if (type == typeof(float))
        {
            return PointType.SingleFloat;
        }

        if (type == typeof(double))
        {
            return PointType.DoubleFloat;
        }

        if (type == typeof(bool))
        {
            return PointType.Boolean;
        }

        if (type == typeof(string))
        {
            return PointType.String;
        }

        if (type == typeof(DateTime))
        {
            return PointType.DateTime;
        }

        if (type == typeof(DateOnly))
        {
            return PointType.DateOnly;
        }

        if (type == typeof(TimeOnly))
        {
            return PointType.TimeOnly;
        }

        if (type == typeof(TimeSpan))
        {
            return PointType.TimeSpan;
        }

        return null;
    }

    public static Type? GetPointNativeType(this PointType type)
    {
        switch (type)
        {
            case PointType.Integer: return typeof(int);
            case PointType.SingleFloat: return typeof(float);
            case PointType.DoubleFloat: return typeof(double);
            case PointType.Boolean: return typeof(bool);
            case PointType.String: return typeof(string);
            case PointType.DateTime: return typeof(DateTime);
            case PointType.DateOnly: return typeof(DateOnly);
            case PointType.TimeOnly: return typeof(TimeOnly);
            case PointType.TimeSpan: return typeof(TimeSpan);
            default: return null;
        }
    }

    public static bool TryCastValueFromString(this PointType pointType, ref object value)
    {
        var valueType = value.GetType();

        if (valueType != typeof(string))
        {
            // We only support converting from string
            return false;
        }

        var nativePointType = pointType.GetPointNativeType();

        if (nativePointType == null)
        {
            // oops, should not happen here but fail safe...
            return false;
        }

        try
        {
            // Convert to expected value type
            var valueWithCorrectType = ((string)value).ConvertToType(nativePointType);
            value = valueWithCorrectType;

            return true;
        }
        catch { }
        {
            // Failed to convert, so return false
            return false;
        }
    }
}
