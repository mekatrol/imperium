﻿using Imperium.Common.Extensions;
using Imperium.Common.Points;

namespace Imperium.Common.Devices;

public class DeviceInstance<T>(string key, string deviceControllerKey, object? data = null, bool enabled = true) : IDeviceInstance
{
    private readonly IList<Point> _points = [];

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

    public Point CreatePoint<TPointNativeType>(string key, string friendlyName, TPointNativeType? value = null) where TPointNativeType : struct
    {
        // Get with default value
        var point = GetPointWithDefault<TPointNativeType>(key);

        // Set friendly name
        point.FriendlyName = friendlyName;

        // Set initial value
        point.Value = value;

        // Return created point
        return point;
    }

    public Point GetPointWithDefault<TType>(string pointKey, Point? defaultValue = null) where TType : struct
    {
        // Try and get existing by key
        var point = _points.SingleOrDefault(p => p.Key == pointKey);

        // If found then return it
        if (point != null)
        {
            return point;
        }

        var pointType = typeof(TType).GetPointType() ?? throw new InvalidOperationException($"The type of point '{typeof(TType).FullName}' is not valid for the type.");

        // Not found then use default or create new
        point = defaultValue ?? new Point(pointKey, pointType) { FriendlyName = pointKey, DeviceKey = Key };

        // Add the new point
        _points.Add(point);

        // Return the point
        return point;
    }

    public override string ToString()
    {
        return $"{{ Key='{Key}', ControllerKey='{ControllerKey}' }}";
    }
}
