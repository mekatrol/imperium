using MQTTnet;

namespace Imperium.Common.Services;

public interface IMqttClientService
{
    Task Tick(Func<MqttApplicationMessageReceivedEventArgs, Task> applicationMessageReceivedAsync, CancellationToken stoppingToken);
}
