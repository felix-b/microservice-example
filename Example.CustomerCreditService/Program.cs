using CustomerService.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICustomerService, CustomerService.BusinessLogic.CustomerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api.credits/customer/increment/{customerId}", async (ICustomerService service, int customerId, [FromBody]IncrementCreditsHttpBody body) => {
    var response = await service.IncrementCustomerCredits(new IncrementCustomerCreditsRequest {CustomerId = customerId, Credits = body.Amount });
    return new { 
        Message=$"OK"
    };
}).WithOpenApi();

app.MapGet("/api.credits/customer/credits/{customerId}", async (ICustomerService service, int customerId) => {
    var response = await service.GetCustomerCredits(new GetCustomerCreditsRequest() {CustomerId=customerId });
    if (response.ClientNotFound)
    {
        return Results.NotFound();
    }
    return Results.Json(response);
}).WithOpenApi();


app.Run("http://localhost:3300");
