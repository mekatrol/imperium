namespace Imperium.Common.Directories;

public class ImperiumDirectories(string baseDirectory)
{
    public string Base { get; } = baseDirectory;

    public string Devices => Path.Combine(Base, "devices");

    public string Scripts => Path.Combine(Base, "scripts");

    public string Switches => Path.Combine(Base, "switches");
}
