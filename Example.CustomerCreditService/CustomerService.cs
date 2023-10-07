using System;

namespace CustomerService.BusinessLogic;

public class CustomerService : ICustomerService
{
    public Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request)
    {
        throw new NotImplementedException();
    }
}
