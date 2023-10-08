using Example.CustomerCreditService.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICustomerService, Example.CustomerCreditService.BusinessLogic.CustomerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api.credits/customer/increment/{customerId}", async (CancellationToken cancel, ICustomerService service, int customerId, [FromBody]IncrementCreditsHttpBody body) => {
    var response = await service.IncrementCustomerCredits(new IncrementCustomerCreditsRequest {CustomerId = customerId, Credits = body.Amount }, cancel);
    return new { 
        Message=$"OK"
    };
}).WithOpenApi();

app.MapGet("/api.credits/customer/credits/{customerId}", async (CancellationToken cancel, ICustomerService service, int customerId) => {
    var response = await service.GetCustomerCredits(new GetCustomerCreditsRequest() {CustomerId=customerId }, cancel);
    if (response.ClientNotFound)
    {
        return Results.NotFound();
    }
    return Results.Json(response);
}).WithOpenApi();


app.Run("http://localhost:3300");

public class IncrementCreditsHttpBody
{
    public double Amount { get; set; }
}