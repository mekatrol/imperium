namespace Imperium.Common.Configuration;

public abstract class VersionedConfiguration
{
    protected readonly Lock _sync = new();

    private int _changeVersion = 1;

    public int ChangeVersion
    {
        get
        {
            lock (_sync)
            {
                return _changeVersion;
            }
        }
        set
        {
            lock (_sync)
            {
                _changeVersion = value;
            }
        }
    }

    protected int IncrementVersion()
    {
        lock (_sync)
        {
            return ++_changeVersion;
        }
    }
}
