namespace Imperium.Common.Points;

public interface IPointDefinition
{
    string Key { get; }

    string FriendlyName { get; }

    Type NativeType { get; }
}
