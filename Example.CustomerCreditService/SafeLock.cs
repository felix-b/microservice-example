namespace Example.CustomerCreditService.BusinessLogic;

internal class SafeLock : IDisposable
{
    private readonly object _sync ;
    private bool _disposed=false ;
    public SafeLock(object resource, string resourceName, int timeout)
    {
        _sync = resource;
        if (!Monitor.TryEnter(_sync, timeout))
        {
            throw new SynchronizationLockException($"Cannot lock {resourceName}!");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Monitor.Exit(_sync);
            _disposed = true;
            
        }
    }
}
