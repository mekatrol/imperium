using System.Text.Json.Serialization;
using System.Text.Json;
using Imperium.Common.Json;

namespace Imperium.Common.Extensions;

public static class JsonSerializerExtensions
{
    public static readonly JsonSerializerOptions ErrorSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions ApiSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    static JsonSerializerExtensions()
    {
        ErrorSerializerOptions.Converters.Add(new ServiceErrorConverter());
        ErrorSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        ApiSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
