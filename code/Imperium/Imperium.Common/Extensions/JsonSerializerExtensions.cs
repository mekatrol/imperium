using Imperium.Common.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

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
