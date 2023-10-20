using System.Dynamic;
using static AllDone.Contracts.FacadeServiceContracts;

namespace AllDone.Services.Facade;

public static class PartitionKey
{
    public static int Get(object request)
    {
        return request switch
        {
            AddUserRequest addUser => addUser.Email.GetHashCode(),
            _ => -1
        };

        /*
        switch(request)
        {
            case AddUserRequest addUser: return addUser.Email.GetHashCode();
            default: return -1;
        }
        */
    }

}
