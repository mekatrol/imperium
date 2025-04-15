using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Imperium.Common.Extensions;

public static class LoggerExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public static void LogError(this ILogger logger, Exception ex)
    {
        logger.LogError(ex, "{Message}", ex.Message);
    }

    public static void LogWarning(this ILogger logger, Exception ex)
    {
        logger.LogWarning(ex, "{Message}", ex.Message);
    }

    public static void LogInformation(this ILogger logger, Exception ex)
    {
        logger.LogInformation(ex, "{Message}", ex.Message);
    }

    public static void LogDebug(this ILogger logger, Exception ex)
    {
        logger.LogDebug(ex, "{Message}", ex.Message);
    }

    public static TObject? DebugDump<TObject>(this ILogger logger, TObject obj)
    {
        var json = obj == null
            ? "null"
            : JsonSerializer.Serialize(obj, _jsonOptions);

        logger.LogDebug("{Message}", $"[{obj?.GetType().Name}]:\r\n{json}");

        return obj;
    }
}
