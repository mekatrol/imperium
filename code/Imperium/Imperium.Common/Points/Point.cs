using Imperium.Common.Extensions;
using Imperium.Common.Json;
using System.Text.Json.Serialization;

namespace Imperium.Common.Points;

[JsonConverter(typeof(PointJsonConverter))]
public class Point
{
    // An object to use as sync lock for multithreaded access
    private readonly Lock _threadLock = new();
    private object? _prevValue = null;
    private bool _hasChanged = false;

    public Point()
    {
        // Parameterless constructor for serialization
    }

    public Point(string deviceKey, DeviceType deviceType, string pointKey, PointType pointType) : this()
    {
        if (string.IsNullOrEmpty(deviceKey))
        {
            throw new ArgumentException(PointJsonConverter.InvalidDeviceKeyMessage);
        }

        if (string.IsNullOrEmpty(pointKey))
        {
            throw new ArgumentException(PointJsonConverter.InvalidKeyMessage);
        }

        DeviceType = deviceType;
        DeviceKey = deviceKey;
        Key = pointKey;
        PointType = pointType;
    }

    /// <summary>
    /// An alias for the point, useful for linking back to source system point names (e.g. xpath for JSON payload in MQTT message) that
    /// allows mapping from source system point name to imperium point name. 
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// The key for the point, unique only with respect to the device it belongs to, e.g. Relay1.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The point value type. 
    /// </summary>
    public PointType PointType { get; set; }

    /// <summary>
    /// The point current state. 
    /// </summary>
    public PointState? PointState { get; set; }

    /// <summary>
    /// The current value of the point. This is not thread safe, use 'GetValue' and 'SetValue' for thread safe operations.
    /// (This is property used for JSON serialization)
    /// </summary>
    public object? Value
    {
        get
        {
            // The current value in order of:
            // device value,
            // override value,
            // control value.
            return DeviceValue ?? OverrideValue ?? ControlValue;
        }
    }

    public object? ControlValue { get; set; } = null;

    /// <summary>
    /// The current device value of the point where point value is different to the imperium control value.
    /// Examples of where the device value can be different to the imperium control value:
    ///   * A device controlled by HTTP requests where something other that Imperium changed the point value.
    ///   * A device where it has local control logic overriding whatever value imperium is sending the device.
    /// This value is only set if the device supports reading abck the current actual value for a point.
    /// </summary>
    public object? DeviceValue { get; set; } = null;

    /// <summary>
    /// If non null then will override the value set by any flow logic and triggers. Useful for overriding a point
    /// to a known state not alterable by control logic.
    /// </summary>
    public object? OverrideValue { get; set; } = null;

    /// <summary>
    /// The previous value when the point was last updated
    /// </summary>
    [JsonIgnore]
    public object? PrevValue
    {
        get
        {
            lock (_threadLock)
            {
                return _prevValue;
            }
        }
    }

    /// <summary>
    /// If true the point cannot be changed (any calls to update via API will fail)
    /// </summary>
    public bool IsReadOnly { get; set; }

    [JsonIgnore]
    public bool HasChanged
    {
        get => _hasChanged;
        set
        {
            lock (_threadLock)
            {
                _hasChanged = value;
                _prevValue = value;
            }
        }
    }

    /// <summary>
    /// The most recent date and time the point was updated, will be null if it has not yet been updated since the server was started.
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// A user display friendly name for the point, eg 'Balcony light'
    /// </summary>
    public string? FriendlyName { get; set; }

    /// <summary>
    /// This is the unique key of the device that owns this point.
    /// It can be 'ImperiumConstants.VirtualDeviceKey' where the point is a virtual in memory point (no actual device)
    /// </summary>
    public string DeviceKey { get; set; } = string.Empty;

    /// <summary>
    /// The type of device the point is controlled by.
    /// </summary>
    public DeviceType DeviceType { get; set; }

    public object? SetValue(object? value, PointValueType valueType)
    {
        // Make sure types match
        if (value != null)
        {
            var pointType = value.GetType().GetPointType();

            if (pointType == null || pointType != PointType)
            {
                // Calls from API clients may serialize as a string, so try to cast.
                if (!PointType.TryCastValueFromString(ref value))
                {
                    throw new InvalidOperationException($"The point value '{value} cannot be converted to the point type '{PointType}' for point '{DeviceKey}.{Key}'.");
                }
            }
        }

        // Update its value
        switch (valueType)
        {
            case PointValueType.Control:
                SetControlValue(value);

                // If this is a virtual point then we also set the Device value
                if (DeviceType == DeviceType.Virtual)
                {
                    SetDeviceValue(value);
                }
                break;

            case PointValueType.Device:
                SetDeviceValue(value);
                break;

            case PointValueType.Override:
                SetOverrideValue(value);
                break;

            default:
                throw new InvalidOperationException($"Unknown point type: '{valueType}'");
        }

        return Value;
    }

    private object? SetControlValue(object? value)
    {
        lock (_threadLock)
        {
            // We use to string because we store and object not a strong type
            // and so the equals operator for native type does not work reliably
            if (value?.ToString() == ControlValue?.ToString())
            {
                return Value;
            }

            _hasChanged = _prevValue != Value;
            _prevValue = Value;
            ControlValue = value;

            // Set last updated
            LastUpdated = DateTime.UtcNow;

            // Updating its value means that it is online
            PointState = Points.PointState.Online;

            return Value;
        }
    }

    private object? SetDeviceValue(object? deviceValue)
    {
        lock (_threadLock)
        {
            DeviceValue = deviceValue;
            return Value;
        }
    }

    private object? SetOverrideValue(object? overrideValue)
    {
        lock (_threadLock)
        {
            OverrideValue = overrideValue;
            return Value;
        }
    }

    public override string ToString()
    {
        return $"{nameof(Key)}='{Key}',{nameof(Value)}='{Value}',{nameof(PointType)}='{PointType}'";
    }
}
