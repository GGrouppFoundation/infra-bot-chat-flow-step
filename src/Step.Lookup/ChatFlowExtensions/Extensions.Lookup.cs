using System;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<TNext> AwaitLookupValue<T, TNext>(
        this ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerAwaitLookupValue<T, TNext>(
        ChatFlow<T> chatFlow,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(default, searchFunc, token),
            mapFlowState);
}