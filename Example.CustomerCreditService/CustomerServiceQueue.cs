using System.Threading.Channels;

namespace Example.CustomerCreditService.BusinessLogic;

public class CustomerServiceQueue : ICustomerService
{
    private readonly ICustomerService _innerService;
    private readonly CancellationToken _hostCancellation;
    private readonly ILogger<CustomerServiceQueue> _logger;
    private readonly Channel<WorkItem> _queue = Channel.CreateBounded<WorkItem>(capacity: 1000);
    
    public CustomerServiceQueue(ICustomerService innerService, CancellationToken hostCancellation, ILogger<CustomerServiceQueue> logger)
    {
        _innerService = innerService;
        _hostCancellation = hostCancellation;
        _logger = logger;
    }

    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request, CancellationToken cancellation)
    {
        var workItem = new WorkItem(request, cancellation);
        await _queue.Writer.WriteAsync(workItem, cancellation); // enqueue
        var response = await workItem.Completion.Task;
        return (GetCustomerCreditsResponse)response;
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    private async Task RunRquestLoop()
    {
        await foreach (var workItem in _queue.Reader.ReadAllAsync(_hostCancellation))
        {
            if (_hostCancellation.IsCancellationRequested)
            {
                break;
            }

            if (workItem.RequestCancellation.IsCancellationRequested)
            {
                continue;
            }

            try
            {
                switch (workItem.Request)
                {
                    case GetCustomerCreditsRequest getCredits:
                        await _innerService.GetCustomerCredits(getCredits, workItem.RequestCancellation);
                        break;
                }
            }
            catch (Exception e)
            {

            }
        }
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
