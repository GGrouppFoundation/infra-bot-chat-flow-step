namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    public bool Equals(ChatFlowStepFailure other)
        =>
        UIMessageComparer.Equals(UIMessage, other.UIMessage) &&
        LogMessageComparer.Equals(LogMessage, other.LogMessage);
}