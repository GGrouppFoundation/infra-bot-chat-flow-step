namespace GGroupp.Infra.Bot.Builder;

public readonly partial struct ChatFlowStepFailure
{
    public static ChatFlowStepFailure FromUI(string uiMessage) => new(uiMessage);
}