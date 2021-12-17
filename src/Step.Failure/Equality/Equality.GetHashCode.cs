using System;

namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    public override int GetHashCode()
        =>
        HashCode.Combine(
            EqualityContract,
            UIMessageComparer.GetHashCode(UIMessage),
            LogMessageComparer.GetHashCode(LogMessage));
}