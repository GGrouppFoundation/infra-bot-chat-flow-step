using System;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitLookupValue<T>(
        ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(default, searchFunc, mapFlowState, token));
}