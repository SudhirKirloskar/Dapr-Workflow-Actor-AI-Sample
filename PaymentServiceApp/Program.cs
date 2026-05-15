using Dapr.Client;
using Dapr;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and Dapr client
builder.Services.AddControllers().AddDapr(); // enables [Topic] attributes
builder.Services.AddDaprClient();
builder.Services.AddLogging();

var app = builder.Build();

// Enable Dapr pub/sub subscription discovery
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();   // <-- CRITICAL

// Endpoint invoked by OrchestratorApp’s PaymentActivity
app.MapPost("/process", (TransferRequest request, ILogger<Program> logger) =>
{
    logger.LogInformation("[PaymentServiceApp] LogInfo: Processing payment: {Amount} from {From} to {To}",
        request.Amount, request.From, request.To);

    Console.WriteLine($"[PaymentServiceApp] Console: Processing payment: {request.Amount} from {request.From} to {request.To}");

    // TODO: Add actual payment logic here (e.g., debit/credit, external API)
    return Results.Ok(new { Status = "Payment processed", request.From, request.To, request.Amount });
})
.WithTopic("pubsub", "payments");  // optional: subscribe to pubsub if you want events

// Required for Dapr to discover subscriptions
app.MapSubscribeHandler();

app.Run();
// Shared model (adjust namespace if you already have one consolidated)
public record TransferRequest(string From, string To, int Amount);