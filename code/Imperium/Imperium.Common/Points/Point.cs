using Imperium.Common.Json;
using System.Text.Json.Serialization;

namespace Imperium.Common.Points;

[JsonConverter(typeof(PointJsonConverter))]
public class Point(string key, PointType pointType)
{
    // An object to use as sync lock for multithreaded access
    private readonly Lock _threadLock = new();
    private object? _value = null;

    public string Key { get; set; } = key;

    public PointType PointType { get; set; } = pointType;

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
                _value = value;
            }
        }
    }

    /// <summary>
    /// The most recent time the point was updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }

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
