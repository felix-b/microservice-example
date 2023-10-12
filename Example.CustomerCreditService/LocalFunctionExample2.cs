namespace Example2;

public class CustomerRepository
{
    private readonly IBillingService _billingService;

    public void DeleteCustomer(int id)
    {
        // delete data from DB....

        RunDeletionLogic(id, true, false);
    }

    private void RunDeletionLogic(int customerId, bool deleteRelations, bool deleteCredits)
    {
        var discoveredRelatedIds = new List<int>();

        if (deleteRelations)
        {
            DiscoverRelatedCustomerIds();
            DeleteRelations();
        }

        if (deleteCredits)
        {
            DeleteCredits();
        }

        void DiscoverRelatedCustomerIds()   
        {
            //
        }

        void DeleteRelations()
        {
            //
        } 

        void DeleteCredits()
        {
            //
        } 
    }

    public interface IBillingService
    {
        void NotifyCustomerDeleted(int id);
    }
}
