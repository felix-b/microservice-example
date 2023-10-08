using CSharpTest.Net.Synchronization;
using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace Example.CustomerCreditService.BusinessLogic;

public class CustomerService : ICustomerService
{
    private readonly IDictionary<int, double> _customerCredits = new Dictionary<int, double>() { { 1, 12.5 }, { 2, 23.4 }, { 3, 15 } };
    private readonly object _sync = new object();

    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request, CancellationToken cancellation)
    {
        bool clientNotFound;
        double customerCredit = 0;
        using (new SafeLock(_sync, "_sync", 10000))
        {
            clientNotFound = !_customerCredits.TryGetValue(request.CustomerId, out customerCredit);
        }

        await Task.Delay(100);

        return new GetCustomerCreditsResponse() { Credits = customerCredit, ClientNotFound = clientNotFound };
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request, CancellationToken cancellation)
    {
        using (new SafeLock(_sync, "_sync", 10000))
        {
            if (!_customerCredits.ContainsKey(request.CustomerId))
            {
                _customerCredits.Add(request.CustomerId, request.Credits);
            }
            else
            {
                _customerCredits[request.CustomerId] += request.Credits;
            }
        }
      
        return new IncrementCustomerCreditsResponse();
    }
}
