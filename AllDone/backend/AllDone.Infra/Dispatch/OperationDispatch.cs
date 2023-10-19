namespace AllDone.Infra.Dispatch;

public class OperationDispatch
{
    private readonly IDispatchMiddleware _firstMiddleware;

    public OperationDispatch(IDispatchMiddleware firstMiddleware)
    {
        _firstMiddleware = firstMiddleware;
    }

    public async Task<TResponse> ExecuteOperationAsync<TRequest, TResponse>(TRequest request) 
        where TRequest : class
        where TResponse : class
    {
        return (TResponse)(await _firstMiddleware.ExecuteOperationAsync(request)) ;

     
    }
}