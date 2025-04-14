using System.Runtime.CompilerServices;
using System.Text.Json;
using Imperium.Common;
using Imperium.Common.Extensions;

namespace Imperium.Models;

public class ImperiumState : IPointState
{
    // An object to use as sync lock for multithreaded access
    private readonly Lock _threadLock = new();

    private bool _serverReadOnlyMode = false;

    // The list of devices currently being managed
    private readonly Dictionary<string, IDeviceController> _deviceControllers = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, IDeviceInstance> _deviceInstances = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, Point> _points = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// When the server read only mode is true then the server will read IO, but not update IO. Useful when testing a server that you
    /// don't want to update the IO
    /// </summary>
    public bool IsReadOnlyMode
    {
        get
        {
            lock (_threadLock)
            {
                return _serverReadOnlyMode;
            }
        }

        set
        {
            lock (_threadLock)
            {
                _serverReadOnlyMode = value;
            }

        }
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

        lock (_threadLock)
        {
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
                var pointKey = $"{deviceInstance.Key}.{point.Key}";

                // Make sure it does not already exist (e.g. the deviceInstance has multiple points with the same key)
                if (_points.ContainsKey(pointKey))
                {
                    throw new InvalidOperationException($"The device with key '{deviceInstance.Key}' has more than one point with the key '{pointKey}'");
                }

                // Add the point
                _points.Add(pointKey, point);
            }
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

        lock (_threadLock)
        {
            // Make sure the key does not already exist
            if (_deviceInstances.ContainsKey(key))
            {
                throw new InvalidOperationException($"A device controller with the key '{key}' already exists.");
            }

            // Add the device
            _deviceControllers.Add(key, deviceController);
        }
    }

    /// <summary>
    /// Get full list of all points
    /// </summary>
    public IList<Point> GetAllPoints()
    {
        lock (_threadLock)
        {
            return [.. _points.Values];
        }
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

        lock (_threadLock)
        {
            IList<Point> devicePoints = [.. _points.Values.Where(p => p.Key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)).ToList()];
            return devicePoints;
        }
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

        lock (_threadLock)
        {
            if (!_points.TryGetValue(devicePointKey, out Point? point))
            {
                return null;
            }

            return point;
        }
    }

    /// <summary>
    /// Get the controller for the specified key, will return null if no matching controller. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceController? GetDeviceController(string key)
    {
        lock (_threadLock)
        {
            if (!_deviceControllers.TryGetValue(key, out IDeviceController? deviceController))
            {
                return null;
            }

            return deviceController;
        }
    }

    /// <summary>
    /// Get the instance for the specified key, will return null if no matching device instance. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceInstance? GetDeviceInstance(string key, bool includePoints)
    {
        lock (_threadLock)
        {
            if (!_deviceInstances.TryGetValue(key, out IDeviceInstance? deviceInstance))
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
    }

    /// <summary>
    /// Get all of the device instances that are currently enabled
    /// </summary>
    public IList<IDeviceInstance> GetEnabledDeviceInstances(bool includePoints)
    {
        lock (_threadLock)
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
    public T? GetPointValue<T>(string deviceKey, string pointKey) where T : class
    {
        // Get a copy of the point
        var point = GetPointCopy(deviceKey, pointKey);

        if (point == null)
        {
            return null;
        }

        var expectedType = typeof(T).GetPointType();

        if (expectedType == null || expectedType != point.PointType)
        {
            throw new InvalidOperationException($"The point type is not compatible with the return value type.");
        }

        return (T?)point.Value;
    }

    /// <inheritdoc/>
    public bool UpdatePointValue(string deviceKey, string pointKey, object? value)
    {
        // Create the key used for points list
        var key = CreateDevicePointKey(deviceKey, pointKey);

        // Try and get the point
        if (_points.TryGetValue(key, out Point? point))
        {
            // Make sure types match
            if (value != null)
            {
                var pointType = value.GetType().GetPointType();

                if (pointType == null || pointType != point.PointType)
                {
                    throw new InvalidOperationException($"The point type is not compatible with the provided value type.");
                }
            }

            // Update its value
            point.Value = value;

            // Return true to indicate that the point value was updated
            return true;
        }

        // Return false to indicate that the point was not updated because it does not exist
        return false;
    }

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    public bool UpdatePointValue(IDeviceInstance deviceInstance, Point point, object? value)
    {
        return UpdatePointValue(deviceInstance.Key, point.Key, value);
    }

    private Point? GetPointCopy(string deviceKey, string pointKey)
    {
        // Create the key used for points list
        var key = CreateDevicePointKey(deviceKey, pointKey);

        // Try and get the point
        if (!_points.TryGetValue(key, out Point? point))
        {
            // Return null to indicate that the point does not exist
            return null;
        }

        // Lock on the point, that way we do not have to lock all points
        // (it is a ref value so this works as all callers will get the same instance)
        lock (point)
        {
            // Return a copy
            return CopyPoint(point);
        }
    }

    private static string CreateDevicePointKey(string deviceKey, string pointKey)
    {
        return $"{deviceKey}.{pointKey}";
    }

    private void SetDeviceInstancePoints(IDeviceInstance deviceInstance)
    {
        lock (_threadLock)
        {
            deviceInstance.Points.Clear();
            var points = GetDevicePoints(deviceInstance.Key);

            foreach (var point in points)
            {
                // We serialize and deserialize to ensure we add a copy
                deviceInstance.Points.Add(CopyPoint(point));
            }
        }
    }

    private static Point CopyPoint(Point point)
    {
        return JsonSerializer.Deserialize<Point>(JsonSerializer.Serialize(point))!;
    }
}
