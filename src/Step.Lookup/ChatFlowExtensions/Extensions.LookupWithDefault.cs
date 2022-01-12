using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<TNext> AwaitLookupValue<T, TNext>(
        this ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            defaultItemsFunc ?? throw new ArgumentNullException(nameof(defaultItemsFunc)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerAwaitLookupValue<T, TNext>(
        ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(defaultItemsFunc, searchFunc, token),
            mapFlowState);

    private static async ValueTask<ChatFlowJump<LookupValue>> GetChoosenValueOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        LookupValueSetDefaultFunc<T>? defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        CancellationToken token)
    {
        var flowState = context.FlowState;

        if (context.StepState is null)
        {
            if (defaultItemsFunc is null)
            {
                return ChatFlowJump.Repeat<LookupValue>(new());
            }

            var defaultValueSet = await defaultItemsFunc.Invoke(flowState, token).ConfigureAwait(false);
            var defaulActivity = context.CreateLookupActivity(defaultValueSet);

            _ = await context.SendActivityAsync(defaulActivity, token).ConfigureAwait(false);
            return context.ToRepeatWithLookupCacheJump(defaultValueSet);
        }

        var choosenItemResult = context.GetChoosenValueOrAbsent();
        if (choosenItemResult.IsPresent)
        {
            return choosenItemResult.Map(ChatFlowJump.Next).OrThrow();
        }

        var searchText = context.Activity.Text;
        if (string.IsNullOrEmpty(searchText))
        {
            return context.RepeatSameStateJump<LookupValue>(default);
        }

        var searchResult = await searchFunc.Invoke(flowState, new(searchText), token).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            var searchFailure = searchResult.FailureOrThrow();
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

            return context.RepeatSameStateJump<LookupValue>(default);
        }

        var lookupValueSet = searchResult.SuccessOrThrow();

        var successActivity = context.CreateLookupActivity(lookupValueSet);
        _ = await context.SendActivityAsync(successActivity, token).ConfigureAwait(false);

        return context.ToRepeatWithLookupCacheJump(lookupValueSet);
    }
}