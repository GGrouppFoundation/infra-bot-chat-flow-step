using System;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            CreateDefaultResultMessage,
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitLookupValue<T>(
        ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(default, searchFunc, resultMessageFactory, mapFlowState, token));
}