namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    public static bool Equals(ChatFlowStepFailure left, ChatFlowStepFailure right)
        =>
        left.Equals(right);

    public static bool operator ==(ChatFlowStepFailure left, ChatFlowStepFailure right)
        =>
        left.Equals(right);

    public static bool operator !=(ChatFlowStepFailure left, ChatFlowStepFailure right)
        =>
        left.Equals(right) is false;

    public override bool Equals(object? obj)
        =>
        obj is ChatFlowStepFailure other && Equals(other);
}