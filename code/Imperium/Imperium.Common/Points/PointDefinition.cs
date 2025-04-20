namespace Imperium.Common.Points;

public class PointDefinition(string key, string friendlyName, Type nativeType) : IPointDefinition
{
    public string Key => key;

    public string FriendlyName => friendlyName;

    public Type NativeType => nativeType;
}
