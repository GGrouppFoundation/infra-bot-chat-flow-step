namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    public bool Equals(ChatFlowStepFailure other)
        =>
        UserMessageComparer.Equals(UserMessage, other.UserMessage) &&
        LogMessageComparer.Equals(LogMessage, other.LogMessage);
}