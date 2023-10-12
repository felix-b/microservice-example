using Example.CustomerCreditService;
using Example.CustomerCreditService.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Console;

Console.WriteLine($"Processor count {Environment.ProcessorCount}");

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
        "/api.credits/customer/increment/{customerId}", 
        async (CancellationToken cancel, ICustomerService service, int customerId, [FromBody]IncrementCreditsHttpBody body) => {
            var response = await service.IncrementCustomerCredits(
                new IncrementCustomerCreditsRequest {CustomerId = customerId, Credits = body.Amount }, 
                cancel);
            
            return new { 
                Message=$"OK"
            };
        }
    ).WithOpenApi();

    app.MapGet(
        "/api.credits/customer/credits/{customerId}", 
        async (CancellationToken cancel, ICustomerService service, int customerId) => {
            var response = await service.GetCustomerCredits(
                new GetCustomerCreditsRequest() {CustomerId=customerId }, 
                cancel);

            if (response.ClientNotFound)
            {
                return Results.NotFound();
            }
            
            return Results.Json(response);
        }
    ).WithOpenApi();
}

void RegisterAllServices()
{
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
}

public class IncrementCreditsHttpBody
{
    public double Amount { get; set; }
}

