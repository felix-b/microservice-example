using CustomerService.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICustomerService, CustomerService.BusinessLogic.CustomerService>();
var app = builder.Build();

app.MapPost("/api.credits/customer/increment/{customerId}", async (ICustomerService service, int customerId, [FromBody]IncrementCreditsHttpBody body) => {
    var response = await service.IncrementCustomerCredits(new IncrementCustomerCreditsRequest {CustomerId = customerId, Credits = body.Amount });
    return new { 
        Message=$"OK"
    };
});

app.MapGet("/api.credits/customer/credits/{customerId}", async (ICustomerService service, int customerId) => {
    var response = await service.GetCustomerCredits(new GetCustomerCreditsRequest() {CustomerId=customerId });
    if (response.ClientNotFound)
    {
        return Results.NotFound();
    }
    return Results.Json(response);
});


app.Run("http://localhost:3300");
