namespace Imperium.Common.Scripting;

public interface IJsonMessageTransformer
{
    Task<string> FromDeviceJson(string json, CancellationToken stoppingToken);

    Task<string> ToDeviceJson(string json, CancellationToken stoppingToken);
}
