namespace Imperium.Common.Utils;

public static class ReflectionUtils
{
    private static readonly Dictionary<Type, string> TypeAliases = new()
    {
        { typeof(void), "void" },
        { typeof(string), "string" },
        { typeof(bool), "bool" },
        { typeof(int), "int" },
        { typeof(long), "long" },
        { typeof(short), "short" },
        { typeof(byte), "byte" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(object), "object" },
        { typeof(char), "char" }
    };

    public static string GetTypeAlias(this Type type)
    {
        if (TypeAliases.TryGetValue(type, out var alias))
        {
            return alias;
        }

        if (type.IsGenericType)
        {
            var genericTypeName = type.Name[..type.Name.IndexOf('`')];
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeAlias));
            return $"{genericTypeName}<{genericArgs}>";
        }

        return type.Name;
    }
}
