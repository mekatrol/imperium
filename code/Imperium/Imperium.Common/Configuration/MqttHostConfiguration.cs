using System.Text.Json;

namespace Imperium.Common.Configuration;

public class MqttHostConfiguration : VersionedConfiguration
{
    private MqttConfiguration? _mqttConfig = null;

    public MqttConfiguration? MqttConfiguration
    {
        get
        {
            lock (_sync)
            {
                return _mqttConfig;
            }
        }

        set
        {
            lock (_sync)
            {
                // Compare using serialized versions so that we are not performing memory instance comparison
                if (JsonSerializer.Serialize(_mqttConfig) != JsonSerializer.Serialize(value))
                {
                    _mqttConfig = value;
                    IncrementVersion();
                }
            }
        }
    }
}
