using Dapr.Client;
using Dapr.Workflow;

public class PaymentActivity : WorkflowActivity<TransferRequest, object>
{

    private readonly DaprClient _dapr;

    // Constructor injection of DaprClient
    public PaymentActivity(DaprClient dapr) => _dapr = dapr;

    public override async Task<object> RunAsync(WorkflowActivityContext context, TransferRequest input)
    {
        Console.WriteLine("[PaymentActivity] STARTED");
        Console.WriteLine($"[PaymentActivity] Requesting payment of {input.Amount} from {input.From} to {input.To}");

        // Invoke PaymentServiceApp via Dapr service invocation
        await _dapr.InvokeMethodAsync<TransferRequest>(
            HttpMethod.Post,
            "paymentservice",   // <-- Dapr app-id for PaymentServiceApp
            "process",          // <-- endpoint exposed in PaymentServiceApp Program.cs
            input);

        Console.WriteLine($"[PaymentActivity] PaymentServiceApp invoked for workflow {context.InstanceId}");

        return true;
    }
}


