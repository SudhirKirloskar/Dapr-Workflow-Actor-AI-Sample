//
// Workflow definition
//
using Dapr.Workflow;

public class TransferWorkflow : Workflow<TransferRequest, string>
{
    public override async Task<string> RunAsync(WorkflowContext context, TransferRequest input)
    {
        /* P1-M4-AI-2026-04-28 */
        /* Phase 1 - Milestone 4 : Introducing AI-based decision making */
        /* Below code is entirely changed for introducing AI-based decision making. 
         * The DecideNextStep method is a placeholder for where AI logic would be implemented. */

        Console.WriteLine($"[OrchestratorApp] Transfer Workflow getting started");

        //var decision = DecideNextStep(input);
        //var decision = await CallAI(input);  //P1-M4-AI-2026-04-28 

        var decision = (
                            await context.CallActivityAsync<string>(
                            nameof(DecisionActivity),
                            input
                        )).Trim().ToLower();
                        //P1-M4-AI-2026-04-29 



        Console.WriteLine($"[Workflow] Decision = {decision}");

        var result = string.Empty;
        if (decision == "payment")
        {
            Console.WriteLine("[DEBUG] Entered PAYMENT branch");

            await context.CallActivityAsync(nameof(PaymentActivity), input);

            Console.WriteLine("[DEBUG] PaymentActivity call completed");

            result = await context.CallActivityAsync<string>(nameof(AccountActivity), input);

            Console.WriteLine("[DEBUG] AccountActivity call completed");
        }
        else if (decision == "fraud")
        {
            Console.WriteLine("[Workflow] Fraud transaction detected. Invoking Notification service.");

            result = "fraud-detected"; 

            var saveInput = new SaveResultInput
            {
                InstanceId = context.InstanceId,
                From = input.From,
                To = input.To,
                Amount = input.Amount,
                Result = result
            };

            await context.CallActivityAsync(nameof(NotificationActivity), saveInput);
        }
        else if (decision == "invalid")
        {
            Console.WriteLine("[Workflow] Invalid transaction detected. Skipping execution.");

            result = "invalid-transaction"; 
        }
        else
        {
            Console.WriteLine($"[Workflow] Unknown decision: {decision}");

            result = "unknown-decision";   
        }

        return result;


        /* 
         * //Code Prior to P1-M4-AI-2026-04-28 
         * 
        // Step 1: Payment (simulate or call external payment activity)
        await context.CallActivityAsync(nameof(PaymentActivity), input);

        // Step 2: Account updates (this activity will call AccountService via Dapr)
        var result = await context.CallActivityAsync<string>(nameof(AccountActivity), input);

        // Step 3: Notification (save result and publish event)
        var saveInput = new SaveResultInput
        {
            InstanceId = context.InstanceId,
            From = input.From,
            To = input.To,
            Amount = input.Amount,
            Result = result
        };

        await context.CallActivityAsync(nameof(NotificationActivity), saveInput);
        Console.WriteLine($"[OrchestratorApp-Program.cs]");
        Console.WriteLine($"[OrchestratorApp] Workflow completed with result: {result}");

        return result;
        */
    }



    /* P1-M4-AI-2026-04-28 */
    async Task<string> CallAI(TransferRequest input)
    {
        using var httpClient = new HttpClient();

        var prompt = $"From:{input.From}, To:{input.To}, Amount:{input.Amount}. Decide: payment, fraud, or invalid. Only return one word.";

        var requestBody = new
        {
            model = "phi3",
            prompt = prompt,
            stream = false
        };

        var response = await httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            requestBody
        );

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[AI RAW] {json}");

        if (json.ToLower().Contains("fraud")) return "fraud";
        if (json.ToLower().Contains("invalid")) return "invalid";

        return "payment";
    }


    /* P1-M4-AI-2026-04-28 */
    // Temporary logic (this is NOT AI yet)
    //Transfering more than 1000 is considered fraud for this example
    string DecideNextStep(TransferRequest input)
    {
        if (    //null checks
                (input == null)                 
                ||
                (string.IsNullOrWhiteSpace(input.From))
                || 
                (string.IsNullOrWhiteSpace(input.To))
                ||
                (input.From == input.To)
            )
            return "invalid";
        else if (input.Amount > 1000) //Xfer of >1000 is fraud for this example
            return "fraud";
        else
            return "payment"; //valid transaction
    }

    
}