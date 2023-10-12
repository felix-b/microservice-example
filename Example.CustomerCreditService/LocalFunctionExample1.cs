namespace Example1;

public class CustomerRepository
{
    private readonly IBillingService _billingService;

    public void DeleteCustomer(int id)
    {
        // delete data from DB....

        // run deletion logic
        var deletionLogic = new CutomerDeletionLogic();
        deletionLogic.CustomerId = id;
        deletionLogic.RunLogic(_billingService, true, false);
    }

    private class CutomerDeletionLogic
    {
        private readonly List<int> _discoveredRelatedIds = new();
        private IBillingService? _billing;

        public void RunLogic(IBillingService billing, bool deleteRelatons, bool deleteCredits)
        {
            _billing = billing;

            if (deleteRelatons)
            {
                DiscoverRelatedCustomerIds();
                DeleteRelations();
            }

            if (deleteCredits)
            {
                DeleteCredits();
            }

            billing.NotifyCustomerDeleted(CustomerId);
        }

        public int CustomerId { get; set;}

        private void DiscoverRelatedCustomerIds()   
        {
            //
        }

        private void DeleteRelations()
        {
            //
        } 

        private void DeleteCredits()
        {
            //
        } 
    }

    public interface IBillingService
    {
        void NotifyCustomerDeleted(int id);
    }
}
