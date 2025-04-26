namespace Imperium.Common.Configuration;

public class MqttHost
{
    public string Key { get; set; } = string.Empty;

    public string Server { get; set; } = string.Empty;

    public int Port { get; set; } = 1883;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

}
