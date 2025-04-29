namespace Imperium.Server.Services;

public interface IAppVersionService
{
    Version ApplicationVersion { get; }

    Guid ExecutionVersion { get; set; }
}
