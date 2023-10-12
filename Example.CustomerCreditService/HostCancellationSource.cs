
using System.Security.Principal;

public class HostCancellationSource : IHostedService
{
    private readonly CancellationTokenSource _hostCancellationSource = new();

    // this will be called when the host is starting up
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // nothing
        return Task.CompletedTask;
    }

    // this will be called when the host is shutting down
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _hostCancellationSource.Cancel(); // set CancellationRequested on the token
        return Task.CompletedTask;
    }

    public CancellationToken Token => _hostCancellationSource.Token;    
}
