namespace Imperium.Common.Models;

public record Device
{
    public string Key { get; set; }

    public string ControllerKey { get; set; }

    public bool Enabled { get; set; }

    public bool Online { get; set; }

    public DateTime? LastCommunication { get; set; }
}
