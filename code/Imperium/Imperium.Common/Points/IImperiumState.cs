namespace Imperium.Common.Points;

public interface IImperiumState
{
    string MqttServer { get; set; }

    string MqttUser { get; set; }

    string MqttPassword { get; set; }
}
