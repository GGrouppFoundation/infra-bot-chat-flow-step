using System;

namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    public override int GetHashCode()
        =>
        HashCode.Combine(
            EqualityContract,
            UserMessageComparer.GetHashCode(UserMessage),
            LogMessageComparer.GetHashCode(LogMessage));
}