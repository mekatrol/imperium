using System.Text.Json;

namespace Imperium.Common.Json;

public static class SerializationExtensions
{
    public static T? SerializeCopy<T>(this T? obj) where T : class
    {
        if (obj == null)
        {
            return null;
        }

        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json);
    }

}
