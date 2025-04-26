namespace Imperium.Common.Directories;

public class ImperiumDirectories(string baseDirectory)
{
    public string Base { get; } = baseDirectory;

    public string Devices => Path.Combine(Base, "devices");

    public string Points => Path.Combine(Base, "points");

    public string Scripts => Path.Combine(Base, "scripts");
}
