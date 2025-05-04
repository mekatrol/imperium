
namespace Imperium.Server.Options;

public class ImperiumStateOptions
{
    public const string SectionName = "State";

    public bool IsReadOnlyMode { get; set; } = false;

    public string ConfigurationPath { get; set; } = string.Empty;
}
