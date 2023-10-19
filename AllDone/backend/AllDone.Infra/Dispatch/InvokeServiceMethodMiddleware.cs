
using Microsoft.Extensions.DependencyInjection;

namespace AllDone.Infra.Dispatch;

public class InvokeServiceMethodMiddleware : IDispatchMiddleware, IServiceMethodMap
{
    private readonly Dictionary<Type, Func<object, Task<object>>> _serviceMethodMap = new();


    public InvokeServiceMethodMiddleware(Action<IServiceMethodMap> mapMethods)
    {
        mapMethods(this);
    }

    Task<object> IDispatchMiddleware.ExecuteOperationAsync(object request)
    {
        if (_serviceMethodMap.TryGetValue(request.GetType(), out var handler))
        {
            return handler(request);
        }

        throw new MissingMethodException(request.GetType().Name);
    }

    void IServiceMethodMap.MapMethod<TRequest, TResponse>(Func<TRequest, Task<TResponse>> method)
    {
        _serviceMethodMap.Add(
            typeof(TRequest), 
            async request => {
                return (await method((TRequest)request))!;
            });
    }
}


public static class QleueueMiddlewareBuilderExtensions
{
    public static void AddQueue(this IOperationDispatchBuild build)
    {
        var nextMiddlewareType = build.FirstMiddlewareType;
        build.Services.AddSingleton<QueueMiddleware>(serviceProvider => new QueueMiddleware(
            next: (IDispatchMiddleware)serviceProvider.GetRequiredService(nextMiddlewareType)
    ));
        build.AddMiddleware<QueueMiddleware>();// I don't like it. Easy to forget
    }
}

