using System.Threading.Channels;

namespace Example.CustomerCreditService.BusinessLogic;

// This class is a decorator which adds request queueing. 
// It handles the requests one by one in a dedicated task (see function 'RunRequestLoop').
// To start and stop the RunRequestLoop task, the class allows hooking itself into web host lifecycle, by implementing IHostedService
public class CustomerServiceQueue : ICustomerService, IHostedService
{
    private readonly ICustomerService _innerService;
    private readonly ILogger<CustomerServiceQueue> _logger;
    private readonly CancellationTokenSource _hostCancellation;
    private readonly Channel<WorkItem> _queue = Channel.CreateBounded<WorkItem>(capacity: 1000);
    private Task _runRequestLoopTask = Task.CompletedTask;

    public CustomerServiceQueue(ICustomerService innerService, ILogger<CustomerServiceQueue> logger)
    {
        _innerService = innerService;
        _logger = logger;
        _hostCancellation = new CancellationTokenSource();

        _logger.LogInformation("CustomerServiceQueue.ctor");
    }

    // Starts the RunRequestLoop task
    // Called when the web host starts. 
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("CustomerServiceQueue.StartAsync will start RunRequestLoop");

        _runRequestLoopTask = RunRequestLoop();

        _logger.LogDebug("CustomerServiceQueue.StartAsync done start RunRequestLoop");

        return Task.CompletedTask;
    }

    // Stops the RunRequestLoop task
    // Called when the web host shuts down. 
    async Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("CustomerServiceQueue.StopAsync");

        // this is the '2-phase stop' pattern: 
        // (1) signal the worker that it has to finish
        // (2) wait for the worker to actually finish
        // this is good if we have multiple workers:
        // - we first signal (1) to all workers, so that meanwhile every worker has time to stop, and all this in parallel
        // - then we wait (2) for all workers to finish. it doesn't matter in which order we wait, what matters is that they all stop in parallel

        // the code of RunRequestLoop 'listens' to _hostCancellation
        // it aborts everything it is doing, as soon as we request cancel here.
        _hostCancellation.Cancel(); 

        _logger.LogDebug("CustomerServiceQueue.StopAsync will wait _runRequestLoopTask");

        // wait until the task is actually finished
        await _runRequestLoopTask;

        _logger.LogDebug("CustomerServiceQueue.StopAsync done wait _runRequestLoopTask");
    }

    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request, CancellationToken cancellation)
    {
        var workItem = new WorkItem(request, cancellation);

        _logger.LogDebug("CustomerServiceQueue.GetCustomerCredits: will enqueue");

        await _queue.Writer.WriteAsync(workItem, cancellation); // enqueue

        _logger.LogDebug("CustomerServiceQueue.GetCustomerCredits: done enqueue");
        
        var response = await workItem.Completion.Task; // wait for the queue to finish processing the request

        _logger.LogDebug("CustomerServiceQueue.GetCustomerCredits: got response");

        return (GetCustomerCreditsResponse)response;
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request, CancellationToken cancellation)
    {
        var workItem = new WorkItem(request, cancellation);

        _logger.LogDebug("CustomerServiceQueue.IncrementCustomerCredits: will enqueue");

        await _queue.Writer.WriteAsync(workItem, cancellation); // enqueue

        _logger.LogDebug("CustomerServiceQueue.IncrementCustomerCredits: done enqueue");

        var response = await workItem.Completion.Task; // wait for the queue to finish processing the request

        _logger.LogDebug("CustomerServiceQueue.IncrementCustomerCredits: got response");

        return (IncrementCustomerCreditsResponse)response;
    }

    private async Task RunRequestLoop()
    {
        _logger.LogInformation("RunRequestLoop: starting task");

        await Task.Yield(); // why? here we want the synchronous part of the fucntion to be broken and a Task be returned.

        try
        {
            await foreach (var workItem in _queue.Reader.ReadAllAsync(_hostCancellation.Token))
            {
                _logger.LogDebug("RunRequestLoop: waiting for next item");

                // the below is not needed because _queue.Reader.ReadAllAsync throws OperationCanceledException when cancellation...
                // ...is requested on _hostCancellation
                //
                //if (_hostCancellation.IsCancellationRequested)
                //{
                //    break;
                //}

                if (workItem.RequestCancellation.IsCancellationRequested)
                {
                    continue;
                }

                try
                {
                    // this cancellation token source combines _hostCancellation and workItem.RequestCancellation
                    // the combined anyCancellation requests cancellation when either _hostCancellation OR workItem.RequestCancellation do
                    var anyCancellation = CancellationTokenSource.CreateLinkedTokenSource(_hostCancellation.Token, workItem.RequestCancellation);
                    
                    await ProcessOneRequest(workItem, anyCancellation.Token);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
            }
        }
        catch (OperationCanceledException)
        {
            // it's ok, _hostCancellation has requested cancellation and that's why _queue.Reader.ReadAllAsync has thrown this exception
        }

        _logger.LogInformation("RunRequestLoop: exiting task");

        // This is a 'local function'. 
        // It's a new feature of C#
        async Task ProcessOneRequest(WorkItem workItem, CancellationToken cancellation)
        {
            switch (workItem.Request)
            {
                case GetCustomerCreditsRequest getCredits:
                    _logger.LogDebug("RunRequestLoop: got GetCustomerCreditsRequest");
                    
                    var getCreditsResponse = await _innerService.GetCustomerCredits(getCredits, cancellation);
                    
                    _logger.LogDebug("RunRequestLoop: will set result on GetCustomerCreditsRequest");
                    
                    workItem.Completion.SetResult(getCreditsResponse);
                    
                    break;

                case IncrementCustomerCreditsRequest incrementCredits:
                    _logger.LogDebug("RunRequestLoop: got IncrementCustomerCreditsRequest");
                    
                    var incrementCreditsResponse = await _innerService.IncrementCustomerCredits(incrementCredits, cancellation);
                    
                    _logger.LogDebug("RunRequestLoop: will set result on IncrementCustomerCreditsRequest");
                    
                    workItem.Completion.SetResult(incrementCreditsResponse);
                    
                    break;

                default:
                    _logger.LogWarning($"RunRequestLoop: Unrecognized request type: '{workItem.Request.GetType().FullName}'");
                    break;
            }
        }
    }

    private record WorkItem(
    TaskCompletionSource<object> Completion,
    object Request,
    CancellationToken RequestCancellation
)
    {
        public WorkItem(object request, CancellationToken cancellation)
            : this(
                Completion: new TaskCompletionSource<object>(),
                Request: request,
                RequestCancellation: cancellation)
        {
        }
    }
}
