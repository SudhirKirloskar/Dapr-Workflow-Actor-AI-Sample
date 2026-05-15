using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Dapr.Actors.Runtime;  //Phase 3: Reference to Actor runtime



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<AccountActor>();
});


var app = builder.Build();
app.MapActorsHandlers(); // Phase 3: critical for actor endpoints.                          
                         //This replaces the older HTTP endpoints (still kept below for just references)
                         //with Actor method calls for balance, deposit, and withdraw methods


// GET /balance/{id} ? Fetch balance from Redis state store
app.MapGet("/balance/{id}", async (string id) =>
{
    using var daprClient = new DaprClientBuilder().Build();
    var balance = await daprClient.GetStateAsync<int>("statestore", id);
    Console.WriteLine($"AccountService: Fetched balance for {id} = {balance}");
    return Results.Ok(new { Account = id, Balance = balance });
});

// POST /deposit ? Increase balance
app.MapPost("/deposit", async (Transaction tx) =>
{
    using var daprClient = new DaprClientBuilder().Build();
    var balance = await daprClient.GetStateAsync<int>("statestore", tx.Account);
    balance += tx.Amount;
    await daprClient.SaveStateAsync("statestore", tx.Account, balance);
    Console.WriteLine($"AccountService: Deposited {tx.Amount} to {tx.Account}, new balance = {balance}");
    return Results.Ok(new { Account = tx.Account, Balance = balance });
});

// POST /withdraw ? Decrease balance
app.MapPost("/withdraw", async (Transaction tx) =>
{
    using var daprClient = new DaprClientBuilder().Build();
    var balance = await daprClient.GetStateAsync<int>("statestore", tx.Account);
    balance -= tx.Amount;
    await daprClient.SaveStateAsync("statestore", tx.Account, balance);

    Console.WriteLine($"AccountService: Withdrew {tx.Amount} from {tx.Account}, new balance = {balance}");
    return Results.Ok(new { Account = tx.Account, Balance = balance });
});

app.Run();

public record Transaction(string Account, int Amount);
