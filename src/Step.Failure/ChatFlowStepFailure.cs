using System;

namespace GGroupp.Infra.Bot.Builder;

public readonly partial struct ChatFlowStepFailure : IEquatable<ChatFlowStepFailure>
{
    private readonly string? uiMessage, logMessage;

    public ChatFlowStepFailure(string uiMessage, string logMessage)
    {
        this.uiMessage = string.IsNullOrEmpty(uiMessage) ? null : uiMessage;
        this.logMessage = string.IsNullOrEmpty(logMessage) ? null : logMessage;
    }

    public ChatFlowStepFailure(string uiMessage)
    {
        this.uiMessage = string.IsNullOrEmpty(uiMessage) ? null : uiMessage;
        logMessage = default;
    }

    public string UIMessage => uiMessage ?? string.Empty;

    public string LogMessage => logMessage ?? string.Empty;
}