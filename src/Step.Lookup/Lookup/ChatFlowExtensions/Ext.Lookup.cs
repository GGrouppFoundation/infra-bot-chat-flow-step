using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(searchFunc);
        ArgumentNullException.ThrowIfNull(resultMessageFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitLookupValue(chatFlow, searchFunc, resultMessageFactory, mapFlowState);
    }

    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(searchFunc);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitLookupValue(chatFlow, searchFunc, CreateDefaultResultMessage, mapFlowState);
    }

    private static ChatFlow<T> InnerAwaitLookupValue<T>(
        ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
    {
        return chatFlow.ForwardValue(InnerNextAsync);

        ValueTask<ChatFlowJump<T>> InnerNextAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.GetChoosenValueOrRepeatAsync(default, searchFunc, resultMessageFactory, mapFlowState, cancellationToken);
    }
}