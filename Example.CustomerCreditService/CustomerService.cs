using System;
using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace CustomerService.BusinessLogic;

public class CustomerService : ICustomerService
{
    IDictionary<int, double> _customerCredits = new Dictionary<int, double>() { { 1, 12.5 }, { 2, 23.4 }, { 3, 15 } };
    object _sync=new object();

    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request)
    {
        var clientNotFound=!_customerCredits.TryGetValue(request.CustomerId,out var customerCredit));

        await Task.Delay(100);

        return new GetCustomerCreditsResponse() { Credits = customerCredit, ClientNotFound = clientNotFound };
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request)
    {
        lock (_sync)
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
        return new IncrementCustomerCreditsResponse();
    }
}
