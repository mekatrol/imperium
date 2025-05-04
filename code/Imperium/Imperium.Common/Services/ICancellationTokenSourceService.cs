namespace Imperium.Common.Services;

public interface ICancellationTokenSourceService
{
    void Add(CancellationTokenSource cancellationTokenSource);

    void CancelAll();
}
