using Imperium.Common.Points;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imperium.Common.Json;

public class PointJsonConverter : JsonConverter<Point>
{
    public const string InvalidPointTypeMessage = $"'{nameof(Point.PointType)}' is required and cannot be null, empty or whitespace. It must of a value from the '{nameof(PointType)}' enum.";
    public const string InvalidDeviceTypeMessage = $"'{nameof(Point.DeviceType)}' is required and cannot be null, empty or whitespace. It must of a value from the '{nameof(DeviceType)}' enum.";
    public const string InvalidKeyMessage = $"'{nameof(Point.Key)}' is required and cannot be null, empty or whitespace.";
    public const string InvalidIdMessage = $"'{nameof(Point.Id)}' is required and cannot be null, empty or whitespace and must not be a all zero GUID value.";
    public const string InvalidIsReadOnlyMessage = $"'{nameof(Point.IsReadOnly)}' must be a valid boolean value.";
    public const string InvalidPointStateMessage = $"'{nameof(Point.PointState)}' is required and cannot be null, empty or whitespace.";
    public const string InvalidDeviceKeyMessage = $"'{nameof(Point.DeviceKey)}' is required and cannot be null, empty or whitespace.";

    public override Point? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var propertyNamingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.Id)), out var idProp) ||
            !Guid.TryParse(idProp.GetString(), out var id) || id == Guid.Empty)
        {
            throw new JsonException(InvalidIdMessage);
        }

        if (!root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.PointType)), out var pointTypeProp) ||
            !Enum.TryParse<PointType>(pointTypeProp.GetString(), true, out var pointType))
        {
            throw new JsonException(InvalidPointTypeMessage);
        }

        if (!root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.DeviceType)), out var deviceTypeProp) ||
            !Enum.TryParse<DeviceType>(deviceTypeProp.GetString(), true, out var deviceType))
        {
            throw new JsonException(InvalidDeviceTypeMessage);
        }

        if (!root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.Key)), out var keyElement))
        {
            throw new JsonException(InvalidKeyMessage);
        }

        if (!root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.IsReadOnly)), out var isReadOnlyProp))
        {
            throw new JsonException(InvalidIsReadOnlyMessage);
        }
        var isReadOnly = isReadOnlyProp.GetBoolean();

        var key = keyElement.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new JsonException(InvalidKeyMessage);
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new JsonException(InvalidKeyMessage);
        }

        PointState? pointState = null;
        if (root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.PointState)), out var pointStateProp))
        {
            var pointStatePropStringValue = pointStateProp.GetString();

            if (pointStatePropStringValue != null)
            {
                if (!Enum.TryParse<PointState>(pointStatePropStringValue, true, out var pointStateNotNullable))
                {
                    throw new JsonException(InvalidPointStateMessage);
                }

                pointState = pointStateNotNullable;
            }
        }

        DateTime? lastUpdated = null;
        if (root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.LastUpdated)), out var lastUpdatedProp))
        {
            var lastUpdatedPropStringValue = lastUpdatedProp.GetString();

            if (lastUpdatedPropStringValue != null)
            {
                if (!DateTime.TryParse(lastUpdatedPropStringValue, out var lastUpdatedNotNullable))
                {
                    throw new JsonException(InvalidPointStateMessage);
                }

                lastUpdated = lastUpdatedNotNullable;
            }
        }

        var deviceKey = string.Empty;
        if (root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.DeviceKey)), out var deviceKeyProp))
        {
            deviceKey = deviceKeyProp.GetString();

            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                throw new JsonException(InvalidDeviceKeyMessage);
            }
        }

        // Trim key any whitespace for safety
        var point = new Point(deviceKey, deviceType, key.Trim(), pointType)
        {
            Id = id,
            LastUpdated = lastUpdated,
            FriendlyName = root.TryGetProperty(propertyNamingPolicy.ConvertName(nameof(Point.FriendlyName)), out var friendlyName) ? friendlyName.GetString() : null,
            IsReadOnly = isReadOnly,
            PointState = pointState
        };

        ReadValueProperty(point, nameof(Point.ControlValue), PointValueType.Control, root, propertyNamingPolicy, options);
        ReadValueProperty(point, nameof(Point.DeviceValue), PointValueType.Device, root, propertyNamingPolicy, options);
        ReadValueProperty(point, nameof(Point.OverrideValue), PointValueType.Override, root, propertyNamingPolicy, options);

        return point;
    }

    public override void Write(Utf8JsonWriter writer, Point point, JsonSerializerOptions options)
    {
        var propertyNamingPolicy = options.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;
        var ignoreNull = options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull);

        writer.WriteStartObject();

        writer.WriteString(propertyNamingPolicy.ConvertName(nameof(Point.Id)), point.Id.ToString());
        writer.WriteString(propertyNamingPolicy.ConvertName(nameof(Point.Key)), point.Key);
        writer.WriteString(propertyNamingPolicy.ConvertName(nameof(Point.PointType)), propertyNamingPolicy.ConvertName(point.PointType.ToString()));
        writer.WriteString(propertyNamingPolicy.ConvertName(nameof(Point.DeviceType)), propertyNamingPolicy.ConvertName(point.DeviceType.ToString()));
        writer.WriteBoolean(propertyNamingPolicy.ConvertName(nameof(Point.IsReadOnly)), point.IsReadOnly);

        if (point.PointState != null || !ignoreNull)
        {
            writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(Point.PointState)));
            if (point.PointState.HasValue)
            {
                writer.WriteStringValue(propertyNamingPolicy.ConvertName(point.PointState.Value.ToString()));
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        if (point.LastUpdated is not null || !ignoreNull)
        {
            writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(Point.LastUpdated)));
            if (point.LastUpdated.HasValue)
            {
                writer.WriteStringValue(point.LastUpdated.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        if (point.FriendlyName is not null || !ignoreNull)
        {
            writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(Point.FriendlyName)));
            if (point.FriendlyName != null)
            {
                writer.WriteStringValue(point.FriendlyName);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        writer.WritePropertyName(propertyNamingPolicy.ConvertName(nameof(Point.DeviceKey)));
        writer.WriteStringValue(point.DeviceKey);

        WriteValueProperty(point.PointType, point.Value, nameof(Point.Value), ignoreNull, writer, propertyNamingPolicy, options);
        WriteValueProperty(point.PointType, point.ControlValue, nameof(Point.ControlValue), ignoreNull, writer, propertyNamingPolicy, options);
        WriteValueProperty(point.PointType, point.DeviceValue, nameof(Point.DeviceValue), ignoreNull, writer, propertyNamingPolicy, options);
        WriteValueProperty(point.PointType, point.OverrideValue, nameof(Point.OverrideValue), ignoreNull, writer, propertyNamingPolicy, options);

        writer.WriteEndObject();
    }

    private static void ReadValueProperty(
        Point point,
        string propertyName,
        PointValueType pointValueType,
        JsonElement root,
        JsonNamingPolicy propertyNamingPolicy,
        JsonSerializerOptions options)
    {
        if (root.TryGetProperty(propertyNamingPolicy.ConvertName(propertyName), out var valueProp))
        {
            object? value = null;

            if (valueProp.GetString() != null)
            {

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

                    case PointType.TimeSpan:
                        value = TimeSpan.Parse(valueProp.GetString()!);
                        break;

                    default:
                        value = JsonSerializer.Deserialize<object>(valueProp.GetRawText(), options);
                        break;
                }
            }

            point.SetValue(value, pointValueType);
        }
    }

    private static void WriteValueProperty(
        PointType pointType,
        object? value, string propertyName,
        bool ignoreNull,
        Utf8JsonWriter writer,
        JsonNamingPolicy propertyNamingPolicy,
        JsonSerializerOptions options)
    {
        if (value != null || !ignoreNull)
        {
            // Custom serialization of Value
            writer.WritePropertyName(propertyNamingPolicy.ConvertName(propertyName));

            if (value != null)
            {
                switch (pointType)
                {
                    case PointType.Integer:
                        writer.WriteNumberValue(Convert.ToInt32(value));
                        break;

                    case PointType.SingleFloat:
                        writer.WriteNumberValue(Convert.ToSingle(value));
                        break;

                    case PointType.DoubleFloat:
                        writer.WriteNumberValue(Convert.ToDouble(value));
                        break;

                    case PointType.Boolean:
                        writer.WriteBooleanValue(Convert.ToBoolean(value));
                        break;

                    case PointType.String:
                        writer.WriteStringValue(value?.ToString());
                        break;

                    case PointType.DateTime:
                        if (value is DateTime dateTime)
                        {
                            writer.WriteStringValue(dateTime);
                        }
                        else if (value is DateTimeOffset dateTimeOffset)
                        {
                            writer.WriteStringValue(dateTimeOffset);
                        }
                        else
                        {
                            writer.WriteNullValue();
                        }
                        break;

                    case PointType.DateOnly:
                        if (value is DateOnly dateOnly)
                        {
                            writer.WriteStringValue(dateOnly.ToString("yyyy-MM-dd"));
                        }
                        else
                        {
                            writer.WriteNullValue();
                        }

                        break;

                    case PointType.TimeOnly:
                        if (value is TimeOnly timeOnly)
                        {
                            writer.WriteStringValue(timeOnly.ToString("HH:mm:ss"));
                        }
                        else
                        {
                            writer.WriteNullValue();
                        }
                        break;

                    case PointType.TimeSpan:
                        if (value is TimeSpan timeSpan)
                        {
                            writer.WriteStringValue(timeSpan.ToString("HH:mm:ss"));
                        }
                        else
                        {
                            writer.WriteNullValue();
                        }

                        break;

                    default:
                        JsonSerializer.Serialize(writer, value, options);
                        break;
                }
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
