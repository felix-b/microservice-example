using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Channels;

namespace AllDone.Infra.Dispatch;

public class QueueMiddleware : IDispatchMiddleware, IHostedService
{
    private readonly IDispatchMiddleware _next;
     private readonly Channel<WorkItem> _queue = Channel.CreateBounded<WorkItem>(capacity: 1000);
    private Task _runRequestLoopTask = Task.CompletedTask;
    private readonly CancellationTokenSource _hostCancellation;

    public QueueMiddleware(IDispatchMiddleware next)
    {
        _next = next;
        _hostCancellation = new CancellationTokenSource();
    }

    public async Task<object> ExecuteOperationAsync(object request)
    {
        // do something... (enqueue... dequeue)
        var cancellation = CancellationToken.None;
        var workItem = new WorkItem(request, cancellation);
        await _queue.Writer.WriteAsync(workItem, cancellation);
        return await workItem.Completion.Task;
     
    }



    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _runRequestLoopTask = RunRequestLoop();
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


    private async Task RunRequestLoop()
    {
        await Task.Yield();
        try
        {
            await foreach (var workItem in _queue.Reader.ReadAllAsync(_hostCancellation.Token))
            {
                var response =await _next.ExecuteOperationAsync(workItem.Request);
                workItem.Completion.SetResult(response);
            }
        }
        catch (Exception) { }
    }

    private record WorkItem(
        TaskCompletionSource<object> Completion,
        object Request,
        CancellationToken RequestCancellation
    ) {
        public WorkItem(object request, CancellationToken cancellation)
            : this(
                Completion: new TaskCompletionSource<object>(), 
                Request: request, 
                RequestCancellation: cancellation)
        {
        }
    }
}
