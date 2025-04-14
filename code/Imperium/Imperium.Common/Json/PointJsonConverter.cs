using System.Text.Json;
using System.Text.Json.Serialization;
using Imperium.Common.Points;

namespace Imperium.Common.Json;

public class PointJsonConverter : JsonConverter<Point>
{
    public const string InvalidPointTypeMessage = $"Missing or invalid '{nameof(Point.PointType)}' during '{nameof(Point)}' deserialization.";
    public const string InvalidKeyMessage = $"'{nameof(Point.Key)}' is required and cannot be null, empty or whitespace.";

    public override Point? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // We deserialize value based on point type
        if (!root.TryGetProperty(nameof(Point.PointType), out var pointTypeProp) ||
            !Enum.TryParse<PointType>(pointTypeProp.GetString(), out var pointType))
        {
            throw new JsonException(InvalidPointTypeMessage);
        }

        if (!root.TryGetProperty(nameof(Point.Key), out var keyElement))
        {
            throw new JsonException(InvalidKeyMessage);
        }

        var key = keyElement.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new JsonException(InvalidKeyMessage);
        }

        // Trim key any whitespace for safety
        var point = new Point(key.Trim(), pointType)
        {
            LastUpdated = root.TryGetProperty(nameof(Point.LastUpdated), out var lastUpdated) ? lastUpdated.GetDateTime() : null,
            FriendlyName = root.TryGetProperty(nameof(Point.FriendlyName), out var friendlyName) ? friendlyName.GetString() : null,
            DeviceKey = root.TryGetProperty(nameof(Point.DeviceKey), out var deviceKey) ? deviceKey.GetString() : null
        };

        if (root.TryGetProperty(nameof(Point.Value), out var valueProp))
        {
            object? value;

            switch (point.PointType)
            {
                case PointType.Integer:
                    value = valueProp.GetInt32();
                    break;

                case PointType.SingleFloat:
                    value = valueProp.GetSingle();
                    break;

                case PointType.DoubleFloat:
                    value = valueProp.GetDouble();
                    break;

                case PointType.Boolean:
                    value = valueProp.GetBoolean();
                    break;

                case PointType.String:
                    value = valueProp.GetString();
                    break;

                case PointType.DateTime:
                    switch (valueProp.ValueKind)
                    {
                        case JsonValueKind.String:
                            value = DateTime.Parse(valueProp.GetString()!);
                            break;
                        case JsonValueKind.Number:
                            value = DateTimeOffset.FromUnixTimeMilliseconds(valueProp.GetInt64()).DateTime;
                            break;
                        default:
                            value = null;
                            break;
                    }
                    break;

                case PointType.DateOnly:
                    value = DateOnly.Parse(valueProp.GetString()!);
                    break;

                case PointType.TimeOnly:
                    value = TimeOnly.Parse(valueProp.GetString()!);
                    break;

                default:
                    value = JsonSerializer.Deserialize<object>(valueProp.GetRawText(), options);
                    break;
            }

            point.Value = value;
        }

        return point;
    }

    public override void Write(Utf8JsonWriter writer, Point point, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(Point.Key), point.Key);
        writer.WriteString(nameof(Point.PointType), point.PointType.ToString());

        if (point.LastUpdated is not null)
        {
            writer.WriteString(nameof(Point.LastUpdated), point.LastUpdated.Value);
        }

        if (point.FriendlyName is not null)
        {
            writer.WriteString(nameof(Point.FriendlyName), point.FriendlyName);
        }

        if (point.DeviceKey is not null)
        {
            writer.WriteString(nameof(Point.DeviceKey), point.DeviceKey);
        }

        if (point.Value != null)
        {
            // Custom serialization of Value
            writer.WritePropertyName(nameof(Point.Value));

            switch (point.PointType)
            {
                case PointType.Integer:
                    writer.WriteNumberValue(Convert.ToInt32(point.Value));
                    break;

                case PointType.SingleFloat:
                    writer.WriteNumberValue(Convert.ToSingle(point.Value));
                    break;

                case PointType.DoubleFloat:
                    writer.WriteNumberValue(Convert.ToDouble(point.Value));
                    break;

                case PointType.Boolean:
                    writer.WriteBooleanValue(Convert.ToBoolean(point.Value));
                    break;

                case PointType.String:
                    writer.WriteStringValue(point.Value?.ToString());
                    break;

                case PointType.DateTime:
                    if (point.Value is DateTime dateTime)
                    {
                        writer.WriteStringValue(dateTime);
                    }
                    else if (point.Value is DateTimeOffset dateTimeOffset)
                    {
                        writer.WriteStringValue(dateTimeOffset);
                    }
                    else
                    {
                        writer.WriteNullValue();
                    }
                    break;

                case PointType.DateOnly:
                    if (point.Value is DateOnly dateOnly)
                        writer.WriteStringValue(dateOnly.ToString("yyyy-MM-dd"));
                    else
                        writer.WriteNullValue();
                    break;

                case PointType.TimeOnly:
                    if (point.Value is TimeOnly timeOnly)
                        writer.WriteStringValue(timeOnly.ToString("HH:mm:ss"));
                    else
                        writer.WriteNullValue();
                    break;

                default:
                    JsonSerializer.Serialize(writer, point.Value, options);
                    break;
            }
        }

        writer.WriteEndObject();
    }
}
