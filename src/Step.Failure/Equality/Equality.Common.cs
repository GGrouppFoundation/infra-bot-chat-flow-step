using System;

namespace GGroupp.Infra.Bot.Builder;

partial struct ChatFlowStepFailure
{
    private static Type EqualityContract
        =>
        typeof(ChatFlowStepFailure);

    private static StringComparer UIMessageComparer
        =>
        StringComparer.Ordinal;

    private static StringComparer LogMessageComparer
        =>
        StringComparer.Ordinal;
}