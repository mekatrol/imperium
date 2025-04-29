namespace Imperium.Server.Services;

internal class AppVersionService : IAppVersionService
{
    private static readonly Lock _sync = new();

    // Each time the server is started a new GUID will be generated which 
    // can be used to force the browser to reload the current page
    // clearing any cached SPA version
    private static Guid _appExecutionVersion = Guid.NewGuid();

    private static readonly Version _appVersion = new(0, 1, 0);

    public Version ApplicationVersion => _appVersion;

    public Guid ExecutionVersion
    {
        get
        {
            lock(_sync)
            {
                return _appExecutionVersion;
            }
        }

        set
        {
            lock(_sync)
            {
                _appExecutionVersion = value;
            }
        }
    }
}
