
public record TransferCompletedEvent
{
    public string WorkflowId { get; init; } = default!;
    public string From { get; init; } = default!;
    public string To { get; init; } = default!;
    public int Amount { get; init; }
    public string Result { get; init; } = default!;
}


