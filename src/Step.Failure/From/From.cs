namespace GGroupp.Infra.Bot.Builder;

public readonly partial struct ChatFlowStepFailure
{
    public static ChatFlowStepFailure From(string uiMessage, string logMessage) => new(uiMessage, logMessage);
}