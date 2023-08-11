using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

public static class ChoiceStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitChoice<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ChoiceStepOption> optionFactory,
        Func<T, ChoiceItem, T> mapFlowState,
        Func<T, T>? mapSkipButtonFlowState = null)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerGetChoiceJumpAsync);

        ValueTask<ChatFlowJump<T>> InnerGetChoiceJumpAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.InternalGetChoiceJumpAsync(optionFactory, mapFlowState, mapSkipButtonFlowState, cancellationToken);
    }
}