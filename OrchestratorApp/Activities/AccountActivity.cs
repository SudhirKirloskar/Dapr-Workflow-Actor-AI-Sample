
using Dapr.Client;
using Dapr.Workflow;
using Dapr.Actors;
using Dapr.Actors.Client;
using AccountActorInterfaces;  //Phase 3: Reference to Actor interfaces

public class AccountActivity : WorkflowActivity<TransferRequest, string>
{
    private readonly DaprClient _dapr;
    public AccountActivity(DaprClient dapr) => _dapr = dapr;

    public override async Task<string> RunAsync(WorkflowActivityContext context, TransferRequest input)
    {
        Console.WriteLine("[AccountActivity] STARTED");
        /* Phase 2 code of HTTP Post replaced with Phase 3 code of Actor method calls. Both are shown here for reference. */

        /*  //Phase 2: HTTP Post calls to AccountServiceApp
         *  // Note: In a real application, you would want to handle errors and ensure that both operations succeed or fail together 
         *  (e.g., using a transaction or compensation logic).
         // Withdraw from sender
        await _dapr.InvokeMethodAsync<Transaction>(
            HttpMethod.Post,
            "accountservice",   // Dapr app-id for AccountServiceApp
            "withdraw",
            new Transaction(input.From, input.Amount));

        // Deposit to receiver
        await _dapr.InvokeMethodAsync<Transaction>(
            HttpMethod.Post,
            "accountservice",
            "deposit",
            new Transaction(input.To, input.Amount));
        */

        // Phase 3: Actor method calls to AccountActor
        //Withdraw from sender
        var actorId = new ActorId(input.From);
        var fromActor = ActorProxy.Create<IAccountActor>(actorId, "AccountActor");
        Console.WriteLine($"[OrchestratorApp] Calling Withdrawal of {input.Amount} from {input.From}");

        await fromActor.Withdraw(input.Amount);
        
        Console.WriteLine($"[OrchestratorApp] Called Withdrawal of {input.Amount} from {input.From}");


        //Deposit to receiver
        var toActorId = new ActorId(input.To);
        var toActor = ActorProxy.Create<IAccountActor>(toActorId, "AccountActor");
        Console.WriteLine($"[OrchestratorApp] Calling Deposit of {input.Amount} into {input.To}");


        await toActor.Deposit(input.Amount);
        
        Console.WriteLine($"[OrchestratorApp] Called Deposit of {input.Amount} into {input.To}");


        Console.WriteLine($"[AccountActivity] Transferred {input.Amount} from {input.From} to {input.To}");
        return "Transfer completed";
    }


}