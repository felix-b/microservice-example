// See https://aka.ms/new-console-template for more information
using AllDone.Infra.Dispatch;
using AllDone.Services.Facade;
using static AllDone.Contracts.FacadeServiceContracts;


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;




var builder = WebApplication.CreateBuilder(args);

RegisterAllServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

MapAllRoutes();

app.Run("http://localhost:3300");


void MapAllRoutes()
{
    app.MapPost(
        "/api/user/add", 
        async (CancellationToken cancel, OperationDispatch dispatch, [FromBody]UserAddHttpBody body) => {
            var request = new AddUserRequest(
                Email: body.Email,
                FullName: body.FullName
            );
            
            //TODO: pass cancel token
            var response = await dispatch.ExecuteOperationAsync<AddUserRequest, AddUserResponse>(request);

            return new { 
                Success = response.Success,
                ErrorCode = response.ErrorCode
            };
        }
    ).WithOpenApi();
}

void RegisterAllServices()
{
    
    //TODO

    /*
    var serviceInstance = new FacadeService();
    var invoker = new InvokeServiceMethodMiddleware(
        map => {
            map.MapMethod<AddUserRequest, AddUserResponse>(serviceInstance.AddUser); // here a dictionary entry is added
        }
    );


    builder.Services.AddLogging(logging =>
        logging.AddSimpleConsole(options => {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.ColorBehavior = LoggerColorBehavior.Disabled;
        })
    );
    Console.WriteLine("Processor count :" + Environment.ProcessorCount.ToString());
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    builder.Services.AddTransient<Example.CustomerCreditService.BusinessLogic.CustomerService>();
    builder.Services.AddTransient<CustomerServiceQueue>(serviceProvider => {
        Console.WriteLine("*** creating instance of CustomerServiceQueue ***");
        return new CustomerServiceQueue(
            innerService: serviceProvider.GetRequiredService<Example.CustomerCreditService.BusinessLogic.CustomerService>(),
            logger: serviceProvider.GetRequiredService<ILogger<CustomerServiceQueue>>()
        );
    });
    
    builder.Services.AddSingleton<MultiPartitionCustomerService>(serviceProvider =>
       new MultiPartitionCustomerService(
           numberOfQueues: Environment.ProcessorCount,
           createBackend: index => serviceProvider.GetRequiredService<CustomerServiceQueue>(),
           logger: serviceProvider.GetRequiredService<ILogger<MultiPartitionCustomerService>>()
       )
   );
    builder.Services.AddSingleton<ICustomerService>(serviceProvider => serviceProvider.GetRequiredService<MultiPartitionCustomerService>());
    builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<MultiPartitionCustomerService>());

    */
}

public class UserAddHttpBody
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

