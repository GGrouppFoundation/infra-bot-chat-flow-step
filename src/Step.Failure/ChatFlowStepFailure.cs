using System;

namespace GGroupp.Infra.Bot.Builder;

public readonly partial struct ChatFlowStepFailure : IEquatable<ChatFlowStepFailure>
{
    private readonly string? userMessage, logMessage;

    public ChatFlowStepFailure(string userMessage, string logMessage)
    {
        this.userMessage = string.IsNullOrEmpty(userMessage) ? default : userMessage;
        this.logMessage = string.IsNullOrEmpty(logMessage) ? default : logMessage;
    }

    public ChatFlowStepFailure(string userMessage)
    {
        this.userMessage = string.IsNullOrEmpty(userMessage) ? default : userMessage;
        logMessage = default;
    }

    public string UserMessage => userMessage ?? string.Empty;

    public string LogMessage => logMessage ?? string.Empty;
}