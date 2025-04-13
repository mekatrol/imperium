using Imperium.Common;

namespace Imperium.Models;

public class ImperiumState
{
    // An object to use as sync lock for multithreaded access
    // TODO: .NET 9+ now has 'System.Threading.Lock' that can be used instead of an object (for better performance) 
    private readonly object _sync = new();

    // The list of devices currently being managed
    private readonly Dictionary<string, IDeviceController> _deviceControllers = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, IDeviceInstance> _deviceInstances = new(StringComparer.OrdinalIgnoreCase);

    // The list of device instances currently being managed, this is typically the definition of the device along with state (but not point state)
    private readonly Dictionary<string, Point> _points = new(StringComparer.OrdinalIgnoreCase);

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

        lock (_sync)
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

        lock (_sync)
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
        lock (_sync)
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

        var deviceIdPrefix = $"{deviceKey}.";

        lock (_sync)
        {
            IList<Point> devicePoints = [.. _points.Values.Where(p => p.Key.StartsWith(deviceIdPrefix, StringComparison.OrdinalIgnoreCase)).ToList()];
            return devicePoints;
        }
    }

    /// <summary>
    /// Get the controller for the specified key, will return null if no matching controller. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceController? GetDeviceController(string key)
    {
        lock (_sync)
        {
            if (!_deviceControllers.TryGetValue(key, out IDeviceController? value))
            {
                return null;
            }

            return value;
        }
    }

    /// <summary>
    /// Get the instance for the specified key, will return null if no matching device instance. 
    /// The key is case insensitive.
    /// </summary>
    public IDeviceInstance? GetDeviceInstance(string key)
    {
        lock (_sync)
        {
            if (!_deviceInstances.TryGetValue(key, out IDeviceInstance? value))
            {
                return null;
            }

            return value;
        }
    }

    /// <summary>
    /// Get all of the device instances that are currently enabled
    /// </summary>
    public IList<IDeviceInstance> GetEnabledDeviceInstances()
    {
        lock (_sync)
        {
            var enabledDeviceInstances = _deviceInstances.Values.Where(di => di.Enabled).ToList();
            return enabledDeviceInstances;
        }
    }
}
