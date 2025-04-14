﻿using Imperium.Common;

namespace Imperium.Models;

public interface IPointState
{
    /// <summary>
    /// Get a single point for a specific device. Returns null if the point was not found for the specified device.
    /// </summary>
    Point? GetDevicePoint(string deviceKey, string pointKey);

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    object? GetPointValue(string deviceKey, string pointKey);

    /// <summary>
    /// Get a point value and cast to the specified type. The specified type must be valid 
    /// for the point type (i.e. you can't cast an integer value to a float, etc)
    /// </summary>
    T? GetPointValue<T>(string deviceKey, string pointKey) where T : class;

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    bool UpdatePointValue(string deviceKey, string pointKey, object? value);

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    bool UpdatePointValue(IDeviceInstance deviceInstance, Point point, object? value);
}