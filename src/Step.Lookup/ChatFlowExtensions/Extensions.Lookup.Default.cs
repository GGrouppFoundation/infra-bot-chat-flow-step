using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            defaultItemsFunc ?? throw new ArgumentNullException(nameof(defaultItemsFunc)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitLookupValue<T>(
        ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(defaultItemsFunc, searchFunc, mapFlowState, token));

    private static async ValueTask<ChatFlowJump<T>> GetChoosenValueOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        LookupValueSetDefaultFunc<T>? defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, T> mapFlowState,
        CancellationToken token)
    {
        if (context.StepState is null)
        {
            if (defaultItemsFunc is null)
            {
                return ChatFlowJump.Repeat<T>(new());
            }

            var defaultOption = await defaultItemsFunc.Invoke(context, token).ConfigureAwait(false);
            if (defaultOption.SkipStep)
            {
                return context.FlowState;
            }

            return await InnerSendLookupActivityAsync(defaultOption).ConfigureAwait(false);
        }

        var cardActionValue = context.GetCardActionValueOrAbsent();
        if (cardActionValue.IsPresent)
        {
            return cardActionValue.FlatMap(context.GetFromLookupCacheOrAbsent).Map(MapLookupValue).Fold(
                ChatFlowJump.Next,
                context.RepeatSameStateJump<T>);
        }

        var searchText = context.Activity.Text;
        if (context.IsNotMessageType() || string.IsNullOrEmpty(searchText))
        {
            return context.RepeatSameStateJump<T>(default);
        }

        var searchResult = await searchFunc.Invoke(context, searchText, token).ConfigureAwait(false);
        return await searchResult.FoldValueAsync(InnerSendLookupActivityAsync, InnerSendFailureActivityAsync).ConfigureAwait(false);

        async ValueTask<ChatFlowJump<T>> InnerSendLookupActivityAsync(LookupValueSetOption option)
        {
            if (option.SkipStep)
            {
                return context.FlowState;
            }

            var successActivity = context.CreateLookupActivity(option);
            _ = await context.SendActivityAsync(successActivity, token).ConfigureAwait(false);

            return context.ToRepeatWithLookupCacheJump<T>(option);
        }

        async ValueTask<ChatFlowJump<T>> InnerSendFailureActivityAsync(BotFlowFailure searchFailure)
        {
            if (string.IsNullOrEmpty(searchFailure.UserMessage) is false)
            {
                var failureActivity = MessageFactory.Text(searchFailure.UserMessage);
                _ = await context.SendActivityAsync(failureActivity, token).ConfigureAwait(false);
            }

            var logMessage = searchFailure.LogMessage;
            if (string.IsNullOrEmpty(logMessage) is false)
            {
                context.Logger.LogError("{logMessage}", logMessage);
            }

            return context.RepeatSameStateJump<T>(default);
        }

        T MapLookupValue(LookupValue lookupValue)
            =>
            mapFlowState.Invoke(context.FlowState, lookupValue);
    }
}