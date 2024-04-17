using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

public static class ChoiceChatFlowExtensions
{
    public static ChatFlow<T> AwaitChoice<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ChoiceStepOption> optionFactory,
        Func<T, ChoiceItem, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return chatFlow.ForwardValue(InnerInvokeAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.InternalGetChoiceJumpAsync(optionFactory, mapFlowState, cancellationToken);
    }

    public static IActivity GetChoiceResultActivity(this ITurnContext context, string name, string value)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.CreateDefaultResultActivity(name.OrEmpty(), value.OrEmpty());
    }
}