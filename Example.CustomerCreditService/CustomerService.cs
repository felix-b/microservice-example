using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace CustomerService.BusinessLogic;

public class CustomerService : ICustomerService
{
    public async Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request)
    {
       await Task.Delay(100);
        return new GetCustomerCreditsResponse() { Credits = 123.45 };
    }

    public async Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request)
    {
        return new IncrementCustomerCreditsResponse();
    }
}
