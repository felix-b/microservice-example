// See https://aka.ms/new-console-template for more information
using AllDone.Infra.Dispatch;
using AllDone.Services.Facade;
using static AllDone.Contracts.FacadeServiceContracts;

Console.WriteLine("Hello, World!");


var serviceInstance = new FacadeService();
var invoker = new InvokeServiceMethodMiddleware(
    map => {
        map.MapMethod<AddUserRequest, AddUserResponse>(serviceInstance.AddUser); // here a dictionary entry is added
    }
);
