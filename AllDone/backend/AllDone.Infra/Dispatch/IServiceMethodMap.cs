namespace AllDone.Infra.Dispatch;

// for every concrete service, we register each of its methods via MapMethod
public interface IServiceMethodMap
{
    void MapMethod<TRequest, TResponse>(Func<TRequest, Task<TResponse>> method);
}
