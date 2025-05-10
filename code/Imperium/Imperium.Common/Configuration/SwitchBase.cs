namespace Imperium.Common.Configuration;

public class SwitchBase
{
    /// <summary>
    /// The unique key for this switch
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Optional display label
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
}
