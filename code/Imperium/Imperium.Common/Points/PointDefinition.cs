namespace Imperium.Common.Points;

public class PointDefinition
{
    public PointDefinition()
    {
        // Empty constructor for serialization
    }

    public PointDefinition(string key, string friendlyName, PointType pointType)
    {
        Key = key;
        FriendlyName = friendlyName;
        PointType = pointType;
    }

    public string Key { get; set; } = string.Empty;

    public string FriendlyName { get; set; } = string.Empty;

    public string? Alias { get; set; }

    public PointType PointType { get; set; }
}
