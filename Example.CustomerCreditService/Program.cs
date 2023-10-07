using CustomerService.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ICustomerService, CustomerService.BusinessLogic.CustomerService>();
var app = builder.Build();

app.MapPost("/api.credits/customer/increment/{customerId}", (ICustomerService service, int customerId, [FromBody]IncrementCreditsHttpBody body) => new { 
    Message=$"increment amount of customer{customerId} by {body.Amount}"
} );

app.MapGet("/api.credits/customer/credits/{customerId}", async (ICustomerService service, int customerId) => new {
    Amount = await service.GetCustomerCredits(new GetCustomerCreditsRequest() {CustomerId=customerId }) 
});


app.Run("http://localhost:3300");
