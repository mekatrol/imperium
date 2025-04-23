namespace Imperium.Server.Services;

internal class AppVersionService : IAppVersionService
{
    // Each time the server is started a new GUID will be generated which 
    // can be used to force the browser to reload the current page
    // clearing any cached SPA version
    private static readonly Guid _appExecutionVersion = Guid.NewGuid();

    private static readonly Version _appVersion = new(0, 1, 0);

    public Version ApplicationVersion => _appVersion;

    public Guid ExecutionVersion => _appExecutionVersion;
}
