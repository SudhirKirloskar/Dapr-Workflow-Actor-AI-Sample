using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dapr.Client;
using Dapr.Workflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dapr.Actors;
using Dapr.Actors.Client;
using AccountActorInterfaces;  //Phase 3: Reference to Actor interfaces

var builder = WebApplication.CreateBuilder(args);

// Register Dapr Workflow runtime and activities
builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<TransferWorkflow>();
    options.RegisterActivity<PaymentActivity>();
    options.RegisterActivity<AccountActivity>();
    options.RegisterActivity<NotificationActivity>();
    options.RegisterActivity<DecisionActivity>(); //P1-M4-AI-2026-04-29
});

// Add Dapr client for state operations and pub/sub
builder.Services.AddSingleton<DaprClient>(_ => new DaprClientBuilder().Build());

// Add controllers if needed later
builder.Services.AddControllers().AddDapr();

var app = builder.Build();

// Enable Dapr pub/sub subscription discovery 
app.UseCloudEvents(); 
app.MapControllers(); 
app.MapSubscribeHandler(); // <-- CRITICAL

app.MapPost("/start-transfer", async (HttpRequest req, DaprWorkflowClient client) =>
{
    var instanceId = Guid.NewGuid().ToString();

    TransferRequest? input = null;

    try
    {
        input = await req.ReadFromJsonAsync<TransferRequest>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[OrchestratorApp] Error reading input: {ex.Message}");
    }

    if (input == null)
    {
        Console.WriteLine("[OrchestratorApp] Using default input.");
        input = new TransferRequest("A", "B", 500);
    }

    Console.WriteLine($"[OrchestratorApp] Input: From={input.From}, To={input.To}, Amount={input.Amount}");

    await client.ScheduleNewWorkflowAsync(
        nameof(TransferWorkflow),
        instanceId,
        input
    );
    Console.WriteLine($"[OrchestratorApp] : TransferRequest sent is From: {input.From}, To: {input.To}, Amount: {input.Amount}");
    return Results.Ok($"Workflow scheduled with ID = {instanceId}");
});

// Status endpoint (reads from state store)
app.MapGet("/status/{instanceId}", async (string instanceId, DaprClient dapr) =>
{
    var key = $"workflow:{instanceId}:result";
    var result = await dapr.GetStateAsync<string>("statestore", key);

    if (result is null)
    {
        Console.WriteLine($"[OrchestratorApp] No result found yet for {instanceId}");
        return Results.Ok(new
        {
            InstanceId = instanceId,
            RuntimeStatus = "RunningOrUnknown",
            Output = (string?)null
        });
    }

    Console.WriteLine($"[OrchestratorApp] Queried result for {instanceId}: {result}");
    return Results.Ok(new
    {
        InstanceId = instanceId,
        RuntimeStatus = "Completed",
        Output = result
    });
});

app.Run();
