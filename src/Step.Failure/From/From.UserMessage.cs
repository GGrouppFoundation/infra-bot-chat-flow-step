namespace GGroupp.Infra.Bot.Builder;

public readonly partial struct ChatFlowStepFailure
{
    public static ChatFlowStepFailure From(string userMessage) => new(userMessage);
}