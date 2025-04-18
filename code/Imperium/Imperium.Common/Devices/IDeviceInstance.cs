﻿using Imperium.Common.Points;

namespace Imperium.Common.Devices;

public interface IDeviceInstance
{
    /// <summary>
    /// This is the unique ket for the device instance. It must be unique across all devices within Imperium.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// This is the unique key of the device controller used to interact with the physical device.
    /// </summary>
    string ControllerKey { get; }

    /// <summary>
    /// Will be set to true if the device instance is currently enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// The points defined for this device instance
    /// </summary>
    IList<Point> Points { get; }

    /// <summary>
    /// Instance specific data
    /// </summary>
    object? Data { get; }

    /// <summary>
    /// Helper method to get the existing point or create it new if it does not exist
    /// </summary>
    Point GetPointWithDefault<TType>(string key, Point? defaultValue = null) where TType : struct;
}
