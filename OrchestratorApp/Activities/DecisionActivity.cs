using Dapr.Workflow;
using System.Net.Http.Json;

/* P1-M4-AI-2026-04-29
 * Phase 1 - Milestone 4 : 
 * Added new activity DecisionActivity 
 * which calls the AI service to get the decision on the transaction.
*/
public class DecisionActivity : WorkflowActivity<TransferRequest, string>
{
    public override async Task<string> RunAsync(WorkflowActivityContext context, TransferRequest input)
    {
        Console.WriteLine($"[DecisionActivity]....Inside RunAsync of DecisionActivity with input: From={input.From}, To={input.To}, Amount={input.Amount}");
        using var httpClient = new HttpClient();

        var prompt = $@"
                        You are a strict decision engine.

                        Rules:
                        - If From == To -> invalid
                        - If Amount > 1000 -> fraud
                        - Otherwise ->payment

                        Transaction:
                        From: {input.From}
                        To: {input.To}
                        Amount: {input.Amount}

                        Return ONLY one word:
                        payment OR fraud OR invalid
                        ";

        var requestBody = new
        {
            model = "phi3",
            prompt = prompt,
            stream = false
        };

        Console.WriteLine($"[DecisionActivity]....Before calling AI service with prompt");

        var response = await httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            requestBody
        );

        Console.WriteLine($"[DecisionActivity]....After calling AI service with prompt");

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[DecisionActivity][AI RAW] {json}");

        string decision = "payment"; // safe default

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("response", out var responseElement))
            {
                var aiText = responseElement.GetString();

                decision = (aiText ?? "")
                            .Trim()
                            .ToLower()
                            .Split('\n')[0]
                            .Trim();
            }
            else
            {
                Console.WriteLine("[DecisionActivity] 'response' field not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DecisionActivity] JSON parse error: {ex.Message}");
        }

        Console.WriteLine($"[DecisionActivity] RAW DECISION = >{decision}<");
        Console.WriteLine($"[DecisionActivity] LENGTH = {decision.Length}");
        Console.WriteLine($"\n[DecisionActivity] FINAL DECISION = '{decision}'\n");

        if (decision == "fraud")
        {
            return "fraud";
        }
        else if (decision == "invalid")
        {
            return "invalid";
        }
        else
        {
            return "payment";
        }
    }
}