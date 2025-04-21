using Imperium.Common.Controllers;
using Imperium.Common.Devices;
using Imperium.Common.Extensions;
using Imperium.Common.Points;

namespace Imperium.Server.State;

/// <summary>
/// NOTE: This class is considered immutable for thread safety. If the configuration changes then
///       a new instance of this class is used (with any included configuration changes)
/// </summary>
internal class ImperiumState : IPointState, IImperiumState
{
    // The list of devices currently being managed
    private readonly Dictionary<string, IDeviceController> _deviceControllers = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, IDeviceInstance> _deviceInstances = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, Point> _points = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// When the server read only mode is true then the server will read IO, but not update IO. Useful when testing a running server 
    /// where you don't want to actually change the physical IO
    /// </summary>
    public bool IsReadOnlyMode { get; set; } = false;

    public string MqttServer { get; set; } = string.Empty;

    public string MqttUser { get; set; } = string.Empty;

    public string MqttPassword { get; set; } = string.Empty;

    public void AddPoint(string deviceKey, Point point)
    {
        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            throw new InvalidOperationException($"The device key must be set");
        }

        var pointKey = CreateDevicePointKey(deviceKey, point.Key);

        _points.Add(pointKey, point);
    }

    /// <summary>
    /// Add the device along with its points.
    /// The key is case insensitive.
    /// </summary>
    public void AddDeviceAndPoints(IDeviceInstance deviceInstance)
    {
        if (string.IsNullOrWhiteSpace(deviceInstance.Key))
        {
            throw new InvalidOperationException($"The device key must be set");
        }

        if (string.IsNullOrWhiteSpace(deviceInstance.ControllerKey))
        {
            throw new InvalidOperationException($"The device controller key must be set");
        }

        // Make sure the key does not already exist
        if (_deviceInstances.ContainsKey(deviceInstance.Key))
        {
            throw new InvalidOperationException($"A device instance with the key '{deviceInstance.Key}' already exists.");
        }

        // Add the device
        _deviceInstances.Add(deviceInstance.Key, deviceInstance);

        // Add the device points (if any are defined)
        foreach (var point in deviceInstance.Points)
        {
            // The unique point key is a combination of the device instance key and the point key
            // This ensures all points are unique within this imperium state object
            var pointKey = CreateDevicePointKey(deviceInstance.Key, point.Key);

            // Make sure it does not already exist (e.g. the deviceInstance has multiple points with the same key)
            if (_points.ContainsKey(pointKey))
            {
                throw new InvalidOperationException($"The device with key '{deviceInstance.Key}' has more than one point with the key '{pointKey}'");
            }

            // Add the point
            _points.Add(pointKey, point);
        }
    }

    /// <summary>
    /// Add a device controller with the specified key.
    /// The key is case insensitive.
    /// </summary>
    public void AddDeviceController(string key, IDeviceController deviceController)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException($"The device controller key must be set");
        }

        // Make sure the key does not already exist
        if (_deviceInstances.ContainsKey(key))
        {
            throw new InvalidOperationException($"A device controller with the key '{key}' already exists.");
        }

        // Add the device
        _deviceControllers.Add(key, deviceController);
    }

    /// <summary>
    /// Get full list of all points
    /// </summary>
    public IList<Point> GetAllPoints()
    {
        return [.. _points.Values];
    }

    /// <summary>
    /// Get list of all points belonging to the specified device key
    /// </summary>
    public IList<Point> GetDevicePoints(string deviceKey)
    {
        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            throw new InvalidOperationException($"{nameof(deviceKey)}' must be a valid key");
        }

        var keyPrefix = $"{deviceKey}.";

        IList<Point> devicePoints = [.. _points.Values.Where(p => p.Key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)).ToList()];
        return devicePoints;
    }

    /// <inheritdoc/>
    public Point? GetDevicePoint(string deviceKey, string pointKey)
    {
        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            throw new InvalidOperationException($"{nameof(deviceKey)}' must be a valid key");
        }

        if (string.IsNullOrWhiteSpace(pointKey))
        {
            throw new InvalidOperationException($"{nameof(pointKey)}' must be a valid key");
        }

        var devicePointKey = $"{deviceKey}.{pointKey}";

        if (!_points.TryGetValue(devicePointKey, out var point))
        {
            return null;
        }

        return point;
    }

    /// <summary>
    /// Get the controller for the specified key, will return null if no matching controller. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceController? GetDeviceController(string controllerKey)
    {
        if (!_deviceControllers.TryGetValue(controllerKey, out var deviceController))
        {
            return null;
        }

        return deviceController;
    }

    /// <summary>
    /// Get the instance for the specified key, will return null if no matching device instance. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceInstance? GetDeviceInstance(string key, bool includePoints)
    {
        if (!_deviceInstances.TryGetValue(key, out var deviceInstance))
        {
            return null;
        }

        if (includePoints)
        {
            SetDeviceInstancePoints(deviceInstance);
        }
        else
        {
            deviceInstance.Points.Clear();
        }

        return deviceInstance;
    }

    /// <summary>
    /// Get all of the device instances that are currently enabled
    /// </summary>
    public IList<IDeviceInstance> GetEnabledDeviceInstances(bool includePoints)
    {
        var enabledDeviceInstances = _deviceInstances.Values.Where(di => di.Enabled).ToList();

        if (includePoints)
        {
            foreach (var deviceInstance in enabledDeviceInstances)
            {
                SetDeviceInstancePoints(deviceInstance);
            }
        }

        return enabledDeviceInstances;
    }

    /// <inheritdoc/>
    public object? GetPointValue(string deviceKey, string pointKey)
    {
        // Get a copy of the point
        var point = GetPointCopy(deviceKey, pointKey);

        // Return its value
        return point?.Value;
    }

    /// <inheritdoc/>
    public T? GetPointValue<T>(string deviceKey, string pointKey)
    {
        // Get a copy of the point
        var point = GetPointCopy(deviceKey, pointKey);

        if (point == null)
        {
            return default;
        }

        var expectedType = typeof(T).GetPointType();

        if (expectedType == null || expectedType != point.PointType)
        {
            throw new InvalidOperationException($"The point type is not compatible with the return value type.");
        }

        return (T?)point.Value;
    }

    /// <inheritdoc/>
    public Point? UpdatePointValue(Guid pointId, object? value)
    {
        var point = _points.Values.SingleOrDefault(p => p.Id == pointId);

        if (point == null)
        {
            return null;
        }

        return UpdatePointValue(point, value);
    }

    /// <inheritdoc/>
    public Point? UpdatePointValue(string deviceKey, string pointKey, object? value)
    {
        // Create the key used for points list
        var key = CreateDevicePointKey(deviceKey, pointKey);

        // Try and get the point
        if (_points.TryGetValue(key, out var point))
        {
            return UpdatePointValue(point, value);
        }

        // Return null to indicate that the point was not updated because it does not exist
        return null;
    }

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    public Point? UpdatePointValue(IDeviceInstance deviceInstance, Point point, object? value)
    {
        return UpdatePointValue(deviceInstance.Key, point.Key, value);
    }

    private Point? GetPointCopy(string deviceKey, string pointKey)
    {
        // Create the key used for points list
        var key = CreateDevicePointKey(deviceKey, pointKey);

        // Try and get the point
        if (!_points.TryGetValue(key, out var point))
        {
            // Return null to indicate that the point does not exist
            return null;
        }

        // Lock on the point, that way we do not have to lock all points
        // (it is a ref value so this works as all callers will get the same instance)
        lock (point)
        {
            return point;
        }
    }

    private static string CreateDevicePointKey(string deviceKey, string pointKey)
    {
        return $"{deviceKey}.{pointKey}";
    }

    private void SetDeviceInstancePoints(IDeviceInstance deviceInstance)
    {
        deviceInstance.Points.Clear();
        var points = GetDevicePoints(deviceInstance.Key);

        foreach (var point in points)
        {
            // We serialize and deserialize to ensure we add a copy
            deviceInstance.Points.Add(point);
        }
    }

    private static Point UpdatePointValue(Point point, object? value)
    {
        // Make sure types match
        if (value != null)
        {
            var pointType = value.GetType().GetPointType();

            if (pointType == null || pointType != point.PointType)
            {
                // Calls from API clients may serialize as a string, so try to cast.
                if (!point.PointType.TryCastValueFromString(ref value))
                {
                    throw new InvalidOperationException($"The point value '{value} cannot be converted to the point type '{point.PointType}'.");
                }
            }
        }

        // Update its value
        point.Value = value;

        // Return the updated point
        return point;
    }
}
