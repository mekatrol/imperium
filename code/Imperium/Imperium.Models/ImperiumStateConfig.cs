namespace Imperium.Models;

public class ImperiumStateConfig
{
    public const string SectionName = "State";

    public bool IsReadOnlyMode { get; set; } = false;

    public IList<string> ApplicationUrls { get; set; } = [];
}
