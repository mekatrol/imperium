using Imperium.Common.Json;
using System.Text.Json.Serialization;

namespace Imperium.Common.Points;

[JsonConverter(typeof(PointJsonConverter))]
public class Point
{
    // An object to use as sync lock for multithreaded access
    private readonly Lock _threadLock = new();
    private object? _value = null;
    private object? _prevValue = null;
    private bool _hasChanged = false;

    public Point()
    {
        Id = Guid.NewGuid();
    }

    public Point(string key, PointType pointType) : this()
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException(PointJsonConverter.InvalidKeyMessage);
        }

        Key = key;
        PointType = pointType;
    }

    /// <summary>
    /// The unique ID of the point. 
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

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
    /// The current value of the point.
    /// </summary>
    public object? Value
    {
        get
        {
            lock (_threadLock)
            {
                return _value;
            }
        }

        set
        {
            lock (_threadLock)
            {
                // We use to string because we store and object not a strong type
                // and so the equals operator for native type does not work reliably
                if (value?.ToString() == _value?.ToString())
                {
                    return;
                }

                _hasChanged = _prevValue != _value;
                _prevValue = _value;
                _value = value;

                // Set last updated
                LastUpdated = DateTime.UtcNow;

                // Updating its value means that it is online
                PointState = Points.PointState.Online;

            }
        }
    }

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
    /// It can be null where the point is a virtual in memory point (no physical device)
    /// </summary>
    public string? DeviceKey { get; set; }

    public override string ToString()
    {
        return $"{nameof(Key)}='{Key}',{nameof(Value)}='{Value}',{nameof(PointType)}='{PointType}'";
    }
}
