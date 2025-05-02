using Imperium.Common.Extensions;
using Imperium.Common.Points;
using System.Reflection;

namespace Imperium.Common.Devices;

public class DeviceInstance(
    string key,
    DeviceType deviceType,
    string deviceControllerKey,
    object? data = null,
    bool enabled = true,
    Assembly? scriptAssembly = null) : IDeviceInstance
{
    private readonly IList<Point> _points = [];

    public DeviceType DeviceType { get; set; } = deviceType;

    public string Key { get; } = key;

    public string ControllerKey { get; } = deviceControllerKey;

    public bool Enabled { get; set; } = enabled;

    public object? Data { get; } = data;

    public IList<Point> Points
    {
        get
        {
            // Return a copy so that it cannot be modified
            return [.. _points];
        }
    }

    public Assembly? ScriptAssembly { get; set; } = scriptAssembly;

    public DateTime? LastCommunication { get; set; }

    public TimeSpan OfflineStatusDuration { get; set; } = TimeSpan.FromMinutes(5);

    public bool Online { get; set; } = false;

    public Point MapPoint(string key, string friendlyName, string? alias, Type nativePointType, object? initialValue = null)
    {
        // Get with default value
        var point = GetPointWithDefault(key, nativePointType);

        // Set friendly name
        point.FriendlyName = friendlyName;

        // Set alias
        point.Alias = alias;

        // Set initial value
        point.SetValue(initialValue, PointValueType.Control);

        // Return created point
        return point;
    }

    public Point GetPointWithDefault<TType>(string key, Point? defaultValue = null) where TType : struct
    {
        return GetPointWithDefault(key, typeof(TType), defaultValue);
    }

    public Point GetPointWithDefault(string pointKey, Type nativePointType, Point? defaultValue = null)
    {
        // Try and get existing by key
        var point = _points.SingleOrDefault(p => p.Key == pointKey);

        // If found then return it
        if (point != null)
        {
            return point;
        }

        var pointType = nativePointType.GetPointType() ?? throw new InvalidOperationException($"The type of point '{nativePointType.FullName}' is not valid for the type.");

        // Not found then use default or create new
        point = defaultValue ?? new Point(Key, DeviceType, pointKey, pointType) { FriendlyName = pointKey };

        // Add the new point
        _points.Add(point);

        // Return the point
        return point;
    }
}
