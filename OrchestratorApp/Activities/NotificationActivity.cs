using Dapr.Client;
using Dapr.Workflow;

public class NotificationActivity : WorkflowActivity<SaveResultInput, object>
{
    private readonly DaprClient _dapr;
    public NotificationActivity(DaprClient dapr) => _dapr = dapr;

    public override async Task<object> RunAsync(WorkflowActivityContext context, SaveResultInput input)
    {
        var key = $"workflow:{input.InstanceId}:result";
        await _dapr.SaveStateAsync("statestore", key, input.Result);
        Console.WriteLine($"[NotificationActivity] Saved result to state with key: {key}");

        // Publish TransferCompletedEvent with full details
        var evt = new TransferCompletedEvent
        {
            WorkflowId = input.InstanceId,
            From = input.From,
            To = input.To,
            Amount = input.Amount,
            Result = input.Result
        };

        await _dapr.PublishEventAsync("pubsub", "transfer-events", evt);
        Console.WriteLine($"[NotificationActivity] Published TransferCompletedEvent for {input.InstanceId}");

        return true;
    }
}