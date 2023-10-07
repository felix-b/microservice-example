using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace CustomerService.BusinessLogic;

public class CustomerService : ICustomerService
{
    private readonly IDictionary<int, double> _customerCredits = new Dictionary<int, double>() { { 1, 12.5 }, { 2, 23.4 }, { 3, 15 } };
    private readonly object _sync = new object();

    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request)
    {
        if (!Monitor.TryEnter(_sync, 1000))
        {
            throw new SynchronizationLockException("Cannot lock!");
        }

        bool clientNotFound;
        double customerCredit = 0;

        try
        {
            clientNotFound = !_customerCredits.TryGetValue(request.CustomerId,out customerCredit);
            
        }
        finally
        {
            Monitor.Exit(_sync);
        }

        await Task.Delay(100);

        return new GetCustomerCreditsResponse() { Credits = customerCredit, ClientNotFound = clientNotFound };
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request)
    {
        if (!Monitor.TryEnter(_sync, 1000))
        {
            throw new SynchronizationLockException("Cannot lock!");
        }

        try
        {
            if (!_customerCredits.TryGetValue(request.CustomerId, out var customerCredit))
            {
                _customerCredits.Add(request.CustomerId, request.Credits);
            }
            else
            {
                _customerCredits[request.CustomerId] += customerCredit;
            }
        }
        finally
        {
            Monitor.Exit(_sync);
        }

        return new IncrementCustomerCreditsResponse();
    }
}
