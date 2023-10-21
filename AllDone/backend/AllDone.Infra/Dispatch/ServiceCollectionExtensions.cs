using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllDone.Infra.Dispatch
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOperationDispatch<TService>(
            this IServiceCollection services,
            Action<TService, IServiceMethodMap> mapMethods,
            Action<IOperationDispatchBuild> build)
            where TService : class
        {
            var builder = new OperationDispatchBuilder(services);
            builder.AddInvokeServiceMethod<TService>(mapMethods);
            build(builder);

            services.AddSingleton<OperationDispatch>(serviceProvider => new OperationDispatch(
                firstMiddleware: (IDispatchMiddleware)serviceProvider.GetRequiredService(builder.FirstMiddlewareType)
            ));
        }
    }


    public static class QueueMiddlewareBuilderExtensions
    {
        public static void AddQueue(this IOperationDispatchBuild build)
        {
            var nextMiddlewareType = build.FirstMiddlewareType;
            build.Services.AddSingleton<QueueMiddleware>(serviceProvider => new QueueMiddleware(
                next: (IDispatchMiddleware)serviceProvider.GetRequiredService(nextMiddlewareType)
            ));
            build.AddMiddleware<QueueMiddleware>();//TODO: I don't like it. Easy to forget
        }
    }


    public static class InvokeServiceMethodMiddlewareBuilderExtensions
    {
        public static void AddInvokeServiceMethod<TService>(this IOperationDispatchBuild build, Action<TService, IServiceMethodMap> mapMethods)
            where TService : class
        {
            build.Services.AddSingleton<InvokeServiceMethodMiddleware>(serviceProvider =>
            {
                var serviceInstance = serviceProvider.GetRequiredService<TService>();
                return new InvokeServiceMethodMiddleware(map => mapMethods(serviceInstance, map));
            });
            build.AddMiddleware<InvokeServiceMethodMiddleware>();
        }
    }

    public static class MultiPartitionMiddlewareBuilderExtensions
    {
        public static void AddMultiPartition(this IOperationDispatchBuild build, int numberOfPartitions,Func<object,int> getPartitionKey)
        {
            build.Services.AddSingleton<MultiPartitionMiddleware>(serviceProvider =>
            {
                var nextMiddlewareType = build.FirstMiddlewareType;
                return new MultiPartitionMiddleware(
                    numberOfPartitions,
                    createBackend: index => (IDispatchMiddleware)serviceProvider.GetRequiredService(nextMiddlewareType),
                    getPartitionKey
                );
            });
            build.AddMiddleware<MultiPartitionMiddleware>();
        }
    }

}
