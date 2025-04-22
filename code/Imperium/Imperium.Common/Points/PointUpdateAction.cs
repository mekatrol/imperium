namespace Imperium.Common.Points;

public enum PointUpdateAction
{
    /// <summary>
    /// Set the control value
    /// </summary>
    Control,

    /// <summary>
    /// Override the point value
    /// </summary>
    Override,

    /// <summary>
    /// Release existing point value override
    /// </summary>
    OverrideRelease,

    /// <summary>
    /// Toggle the point state value, only valid for Boolean type points
    /// </summary>
    Toggle
}
