namespace AllDone.Infra.Dispatch;

public class OperationDispatch
{
    private readonly IDispatchMiddleware _firstMiddleware;

    public OperationDispatch(IDispatchMiddleware firstMiddleware)
    {
        _firstMiddleware = firstMiddleware;
    }

    public Task<TResponse> ExecuteOperationAsync<TRequest, TResponse>(TRequest request) 
        where TRequest : class
        where TResponse : class
    {
        // call first middleware
        throw new NotImplementedException();  
    }
}