namespace Imperium.Common.Points;

public class PointUpdateValueModel
{
    /// <summary>
    /// The ID of the point that should be updated.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The new value as a string, it must be convertable to a point type value.
    /// </summary>
    public string Value { get; set; }

    public override string ToString()
    {
        return $"{Id}={Value}";
    }
}
