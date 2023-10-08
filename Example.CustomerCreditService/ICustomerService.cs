namespace Example.CustomerCreditService.BusinessLogic;

public interface ICustomerService
{
    Task<GetCustomerCreditsResponse> GetCustomerCredits(GetCustomerCreditsRequest request, CancellationToken cancellation);
    Task<IncrementCustomerCreditsResponse> IncrementCustomerCredits(IncrementCustomerCreditsRequest request, CancellationToken cancellation);
}

public class IncrementCustomerCreditsRequest
{
    public int CustomerId { get; set; }
    public double Credits { get; set; }
}

public class IncrementCustomerCreditsResponse
{
}

public class GetCustomerCreditsRequest
{
    public int CustomerId { get; set; }
}

public class GetCustomerCreditsResponse
{
    public double Credits { get; set; }
    public bool ClientNotFound {  get; set; }
}