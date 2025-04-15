namespace Imperium.Common.Extensions;

public static class ReflectionExtensions
{
    public static object ConvertToType(this string value, Type targetType)
    {
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value);
        }

        if (targetType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        return Convert.ChangeType(value, targetType);
    }

    public static T ConvertToType<T>(this string value)
    {
        return (T)ConvertToType(value, typeof(T));
    }
}
