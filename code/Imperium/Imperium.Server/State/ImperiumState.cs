using Imperium.Common.Configuration;
using Imperium.Common.DeviceControllers;
using Imperium.Common.Devices;
using Imperium.Common.Events;
using Imperium.Common.Exceptions;
using Imperium.Common.Extensions;
using Imperium.Common.Models;
using Imperium.Common.Points;
using System.Collections.Concurrent;

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

    private readonly Dictionary<string, SwitchEntity> _switches = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Dashboard> _dashboards;

    private readonly ConcurrentQueue<SubscriptionEvent> _changeEvents = new();

    public ImperiumState()
    {
        _dashboards = new Dictionary<string, Dashboard>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "main",
                new Dashboard
                {
                    Name = "main",
                    Items = [
                        new DashboardItem {
                            ComponentName = "StatusIconCard",
                            Column = 1,
                            ColumnSpan = 2,
                            Row = 1,
                            RowSpan = 4,
                            Props = new {
                                IconOff = "devices_off",
                                IconOn = "devices",
                                ColorOff = "#991503",
                                DeviceKey = "browser",
                                PointKey = "server.status"
                            }
                        },
                        new DashboardItem {
                            ComponentName = "TimeCard",
                            Column = 3,
                            ColumnSpan = 12,
                            Row = 1,
                            RowSpan = 4,
                            CssClass = "time-card-cell"
                        },
                        new DashboardItem {
                            ComponentName = "StatusIconCard",
                            Column = 15,
                            ColumnSpan = 2,
                            Row = 1,
                            RowSpan = 4,
                            Props = new {
                                IconOff = "dark_mode",
                                IconOn = "wb_sunny",
                                ColorOn = "#ffff00",
                                ColorOff = "#aaa",
                                DeviceKey = "browser",
                                PointKey = "server.status"
                            }
                        },
                        new DashboardItem{
                            ComponentName =  "DashboardSwitchCell",
                            Column = 1,
                            ColumnSpan = 4,
                            Row = 11,
                            RowSpan = 4,
                            CssClass = "padded-cell",
                            Props= new {
                                Icon = "handyman",
                                ValueDeviceKey = "virtual",
                                ValuePointKey = "panic"
                            }
                        },
                        new DashboardItem
                        {
                            ComponentName = "DashboardSwitchCell",
                            Column = 5,
                            ColumnSpan = 8,
                            Row = 11,
                            RowSpan = 4,
                            CssClass = "padded-cell",
                            Props= new
                            {
                                Icon = "e911_emergency",
                                ValueDeviceKey = "virtual",
                                ValuePointKey = "panic"
                            }
                          },
                        new DashboardItem
                        {
                            ComponentName = "DashboardSwitchCell",
                            Column = 13,
                            ColumnSpan = 4,
                            Row = 11,
                            RowSpan = 4,
                            CssClass = "padded-cell",
                            Props = new
                            {
                                Icon = "pets",
                                ValueDeviceKey = "virtual",
                                ValuePointKey = "panic"
                            }
                         }
                    ]
                }
            }
        };

        var cellCol = 1;
        var cellRow = 5;

        void addNextCell(string icon, string deviceKey, string pointKey)
        {
            _dashboards["main"].Items.Add(
                new DashboardItem
                {
                    ComponentName = "DashboardSwitchCell",
                    Column = cellCol,
                    ColumnSpan = 4,
                    Row = cellRow,
                    RowSpan = 3,
                    CssClass = "padded-cell",
                    Props = new
                    {
                        Icon = icon,
                        ValueDeviceKey = deviceKey,
                        ValuePointKey = pointKey
                    }
                });

            cellCol += 4;

            if (cellCol > 16)
            {
                cellCol = 1;
                cellRow += 3;
            }
        }

        addNextCell("garage", "device.carport.powerboard", "Relay1");
        addNextCell("light", "device.frontdoorlight", "Relay");
        addNextCell("looks_6", "device.housenumberlight", "Relay");
        addNextCell("checkroom", "device.clothesline", "Relay");
        addNextCell("heat_pump_balance", "virtual", "water.pumps");
        addNextCell("light", "device.alfrescolight", "Relay");
        addNextCell("light", "device.kitchen.light", "Relay");
        addNextCell("light", "device.kitchenview.powerboard", "Relay1");
    }

    /// <summary>
    /// When the server read only mode is true then the server will read IO, but not update IO. Useful when testing a running server 
    /// where you don't want to actually change the device IO
    /// </summary>
    public bool IsReadOnlyMode { get; set; } = false;

    /// <summary>
    /// The websocket URL to listen on
    /// </summary>
    public Uri WebSocketUri { get; set; } = new Uri("ws://localhost");

    /// <summary>
    /// The queued change events
    /// </summary>
    public ConcurrentQueue<SubscriptionEvent> ChangeEvents => _changeEvents;

    public void AddPoint(Point point)
    {
        if (string.IsNullOrWhiteSpace(point.DeviceKey))
        {
            throw new InvalidOperationException($"The point device key must be set");
        }

        if (string.IsNullOrWhiteSpace(point.Key))
        {
            throw new InvalidOperationException($"The point key must be set");
        }

        var pointKey = CreateDevicePointKey(point.DeviceKey, point.Key);

        if (_points.ContainsKey(pointKey))
        {
            throw new Exception($"A point with device key '{point.DeviceKey}' and point key '{point.Key}' has already been added.");
        }

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

    public IList<IDeviceInstance> GetAllDevices()
    {
        return _deviceInstances.Values.ToList();
    }

    public IList<SwitchEntity> GetSwitches(IList<string>? keys = null)
    {
        if (keys == null || keys.Count == 0)
        {
            // Return all switches if keys null or empty
            return _switches.Values.ToList();
        }

        // Return filtered set of switches
        return _switches.Values
            .Where(s => keys.Contains(s.Key))
            .ToList();
    }

    public SwitchEntity AddSwitch(SwitchConfiguration switchConfig)
    {
        // Key must be valid
        if (string.IsNullOrWhiteSpace(switchConfig.Key))
        {
            throw new InvalidOperationException($"The switch key cannot be empty.");
        }

        // Remove surrounding whitespace
        switchConfig.Key = switchConfig.Key.Trim();

        // Make sure the key does not already exist
        if (_switches.ContainsKey(switchConfig.Key))
        {
            throw new InvalidOperationException($"A switch with the key '{switchConfig.Key}' already exists.");
        }

        var switchEntity = new SwitchEntity
        {
            Key = switchConfig.Key,
            Label = switchConfig.Label,
            Description = switchConfig.Description,
            State = switchConfig.State ?? SwitchState.Offline
        };

        _switches.Add(switchEntity.Key, switchEntity);

        return switchEntity;
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
    /// Get a copy of the defined device controllers.
    /// </summary>
    public IDictionary<string, IDeviceController> GetDeviceControllers()
    {
        return _deviceControllers.ToDictionary(StringComparer.OrdinalIgnoreCase);
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
    public Point? UpdatePointValue(string deviceKey, string pointKey, object? value, PointValueType valueType)
    {
        // Create the key used for points list
        var key = CreateDevicePointKey(deviceKey, pointKey);

        // Try and get the point
        if (_points.TryGetValue(key, out var point))
        {
            var oldValue = point.Value;
            point.SetValue(value, valueType);

            if (oldValue != point.Value)
            {
                // The value has changed so notify any subscribers
                var valueChangeEvent = new PointSubscriptionEvent(
                    SubscriptionEventType.ValueChange,
                    SubscriptionEventEntityType.Point,
                    point);

                _changeEvents.Enqueue(valueChangeEvent);
            }

            return point;
        }

        // Return null to indicate that the point was not updated because it does not exist
        return null;
    }

    /// <summary>
    /// Update a single point value, this is done in a thread safe way.
    /// </summary>
    public Point? UpdatePointValue(IDeviceInstance deviceInstance, Point point, object? value, PointValueType valueType)
    {
        return UpdatePointValue(deviceInstance.Key, point.Key, value, valueType);
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

    public IList<Dashboard> GetAllDashboards()
    {
        return _dashboards.Values.ToList();
    }

    public Dashboard GetDashboard(string name)
    {
        if (!_dashboards.TryGetValue(name, out var dashboard))
        {
            throw new NotFoundException($"A dashboard with the name '{name}' was not found");
        }

        return dashboard;
    }
}
