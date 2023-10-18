namespace AllDone.Infra.Dispatch;

public class QueueMiddleware : IDispatchMiddleware
{
    private readonly IDispatchMiddleware _next;
     private readonly Channel<WorkItem> _queue = Channel.CreateBounded<WorkItem>(capacity: 1000);
    public QueueMiddleware(IDispatchMiddleware next)
    {
        _next = next;
    }

    public Task<object> ExecuteOperationAsync(object request)
    {
        // do something... (enqueue... dequeue)
        return _next.ExecuteOperationAsync(request); // chain to next middleware
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
