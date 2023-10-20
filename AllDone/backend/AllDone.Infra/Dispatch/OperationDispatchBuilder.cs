using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllDone.Infra.Dispatch
{
    internal class OperationDispatchBuilder : IOperationDispatchBuild
    {
        public OperationDispatchBuilder(IServiceCollection services)
        {
            Services = services;
            FirstMiddlewareType = typeof(InvokeServiceMethodMiddleware);
            LastMiddlewareType = FirstMiddlewareType;
        }

        public void AddMiddleware<T>() where T : IDispatchMiddleware
        {
            FirstMiddlewareType = typeof(T); 
        }

        public Type FirstMiddlewareType { get; private set; }
        
        public Type LastMiddlewareType { get; }

        public IServiceCollection Services { get; }

    }

    public interface IOperationDispatchBuild
    {
        void AddMiddleware<T>() where T : IDispatchMiddleware;
        Type LastMiddlewareType { get; }
        Type FirstMiddlewareType { get; }
        IServiceCollection Services { get; }
    }
    
}
