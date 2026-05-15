using Microsoft.AspNetCore.Mvc;
using Dapr;

namespace NotificationServiceApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        [Topic("pubsub", "transfer-events")]
        [HttpPost("transfer-events")]
        public IActionResult HandleTransferEvent([FromBody] TransferCompletedEvent evt)
        {
            Console.WriteLine($"[NotificationService] Received transfer event: WorkflowId={evt.WorkflowId}, From={evt.From}, To={evt.To}, Amount={evt.Amount}, Result={evt.Result}");
            return Ok();
        }
    }

    public record TransferCompletedEvent(
        string WorkflowId,
        string From,
        string To,
        int Amount,
        string Result
    );
}
