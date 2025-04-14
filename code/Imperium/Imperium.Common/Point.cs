namespace Imperium.Common;

public class Point(PointType pointType)
{
    // An object to use as sync lock for multithreaded access
    private readonly Lock _threadLock = new();
    private object? _value = null;

    public string Key { get; set; } = string.Empty;

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
        return $"Id={Key}, Value = {Value}, Type = {PointType}";
    }
}
