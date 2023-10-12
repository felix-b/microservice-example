using Example.CustomerCreditService.BusinessLogic;
using System.Collections.Immutable;

namespace Example.CustomerCreditService
{
    public class MultiPartitionCustomerService : ICustomerService, IHostedService
    {
        private readonly int _numberOfQueues;
        private readonly ImmutableArray<ICustomerService> _backends;
        private readonly ILogger<MultiPartitionCustomerService> _logger;

        public MultiPartitionCustomerService(int numberOfQueues, Func<int, ICustomerService> createBackend,  ILogger<MultiPartitionCustomerService> logger)
        {
            _numberOfQueues = numberOfQueues;
            _backends = Enumerable.Range(0, numberOfQueues).Select(createBackend).ToImmutableArray();
            _logger=logger;
        }

         Task<GetCustomerCreditsResponse> ICustomerService.GetCustomerCredits(GetCustomerCreditsRequest request, CancellationToken cancellation)
        {
           return  _backends[request.CustomerId.GetHashCode() %_numberOfQueues].GetCustomerCredits(request, cancellation);
        }

        Task<IncrementCustomerCreditsResponse> ICustomerService.IncrementCustomerCredits(IncrementCustomerCreditsRequest request, CancellationToken cancellation)
        {
            return _backends[request.CustomerId.GetHashCode() % _numberOfQueues].IncrementCustomerCredits(request, cancellation);
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
