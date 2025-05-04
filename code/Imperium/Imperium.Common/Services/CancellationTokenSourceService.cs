namespace Imperium.Common.Services;

internal class CancellationTokenSourceService : ICancellationTokenSourceService
{
    private readonly List<CancellationTokenSource> _cancellationTokenSources = [];
    private readonly Lock _lock = new();

    public void Add(CancellationTokenSource cancellationTokenSource)
    {
        lock (_lock)
        {
            _cancellationTokenSources.Add(cancellationTokenSource);
        }
    }

    public void CancelAll()
    {
        lock (_lock)
        {
            foreach (var cancellationTokenSource in _cancellationTokenSources)
            {
                cancellationTokenSource.Cancel();
            }

            _cancellationTokenSources.Clear();
        }
    }
}
