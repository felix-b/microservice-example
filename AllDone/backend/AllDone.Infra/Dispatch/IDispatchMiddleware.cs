namespace AllDone.Infra.Dispatch;

public interface IDispatchMiddleware
{
    Task<object> ExecuteOperationAsync(object request);
}
