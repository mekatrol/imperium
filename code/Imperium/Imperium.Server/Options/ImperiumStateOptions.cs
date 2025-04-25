namespace Imperium.Server.Options;

public class ImperiumStateOptions
{
    public const string SectionName = "State";

    public bool IsReadOnlyMode { get; set; } = false;

    public IList<string> ApplicationUrls { get; set; } = [];

    public string MqttServer { get; set; } = string.Empty;

    public int MqttPort { get; set; } = 1883;

    public string MqttUser { get; set; } = string.Empty;

    public string MqttPassword { get; set; } = string.Empty;

    public string ConfigurationPath { get; set; } = string.Empty;
}
