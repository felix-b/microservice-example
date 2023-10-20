using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllDone.Infra.Dispatch
{
    internal class MultiPartitionMiddleware : IDispatchMiddleware, IHostedService
    {
        private readonly int _numberOfPartitions;
        private readonly ImmutableArray<IDispatchMiddleware> _backends;
        private readonly Func<object, int> _getPartitionKey;

        public MultiPartitionMiddleware(int numberOfPartitions,Func<int,IDispatchMiddleware> createBackend, Func<object,int> getPartitionKey)
        {
            _numberOfPartitions=numberOfPartitions;
            _backends = Enumerable.Range(0, numberOfPartitions).Select(createBackend).ToImmutableArray();
            _getPartitionKey= getPartitionKey;
        }
        async Task<object> IDispatchMiddleware.ExecuteOperationAsync(object request)
        {
            var index=_getPartitionKey(request)%_numberOfPartitions;
            return await _backends[index].ExecuteOperationAsync(request);
        }

        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            foreach (var backend in _backends.OfType<IHostedService>())
            {
                await backend.StartAsync(cancellationToken);
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(
                _backends
                .OfType<IHostedService>()
                .Select(service => service.StopAsync(cancellationToken)));
        }
    }
}
