using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dapr.Client;

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

// Optional health endpoint
app.MapGet("/", () => "NotificationService running");

app.Run();
