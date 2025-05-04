namespace Imperium.Common.Events;

public class ValueChangeEvent(string key, object? value)
{
    public string Key => key;

    public object? Value => value;
}
