using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitLookupValue<T>(
        this ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitLookupValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            defaultItemsFunc ?? throw new ArgumentNullException(nameof(defaultItemsFunc)),
            searchFunc ?? throw new ArgumentNullException(nameof(searchFunc)),
            resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

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
            CreateDefaultResultMessage,
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitLookupValue<T>(
        ChatFlow<T> chatFlow,
        LookupValueSetDefaultFunc<T> defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(defaultItemsFunc, searchFunc, resultMessageFactory, mapFlowState, token));

    private static async ValueTask<ChatFlowJump<T>> GetChoosenValueOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        LookupValueSetDefaultFunc<T>? defaultItemsFunc,
        LookupValueSetSearchFunc<T> searchFunc,
        Func<IChatFlowContext<T>, LookupValue, string> resultMessageFactory,
        Func<T, LookupValue, T> mapFlowState,
        CancellationToken token)
    {
        if (context.StepState is null)
        {
            if (defaultItemsFunc is null)
            {
                return ChatFlowJump.Repeat<T>(new());
            }

            await context.SetTypingStatusAsync(token).ConfigureAwait(false);

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
            var actionValueId = cardActionValue.OrThrow();
            return await context.GetFromLookupCacheOrAbsent(actionValueId).FoldValueAsync(NextAsync, RepeatAsync).ConfigureAwait(false);
        }

        var searchText = context.Activity.Text;
        if (context.IsNotMessageType() || string.IsNullOrEmpty(searchText))
        {
            return context.RepeatSameStateJump();
        }

        await context.SetTypingStatusAsync(token).ConfigureAwait(false);

        var searchResult = await searchFunc.Invoke(context, searchText, token).ConfigureAwait(false);
        return await searchResult.FoldValueAsync(InnerSendLookupActivityAsync, InnerSendFailureActivityAsync).ConfigureAwait(false);

        async ValueTask<ChatFlowJump<T>> InnerSendLookupActivityAsync(LookupValueSetOption option)
        {
            if (option.SkipStep)
            {
                return context.FlowState;
            }

            var successActivity = context.CreateLookupActivity(option);
            var resource = await context.SendActivityAsync(successActivity, token).ConfigureAwait(false);

            return context.ToRepeatWithLookupCacheJump(resource, option);
        }

        async ValueTask<ChatFlowJump<T>> InnerSendFailureActivityAsync(BotFlowFailure searchFailure)
        {
            if (string.IsNullOrEmpty(searchFailure.UserMessage) is false)
            {
                var failureActivity = MessageFactory.Text(searchFailure.UserMessage);
                _ = await context.SendActivityAsync(failureActivity, token).ConfigureAwait(false);
            }

            var logMessage = searchFailure.LogMessage;
            if (string.IsNullOrEmpty(logMessage) is false || searchFailure.SourceException is not null)
            {
                context.Logger.LogError(searchFailure.SourceException, "{logMessage}", logMessage);

                var properties = new Dictionary<string, string>
                {
                    ["flowId"] = context.ChatFlowId,
                    ["message"] = searchFailure.LogMessage
                };

                if (searchFailure.SourceException is not null)
                {
                    properties["errorMessage"] = searchFailure.SourceException.Message ?? string.Empty;
                    properties["errorType"] = searchFailure.SourceException.GetType().FullName ?? string.Empty;
                    properties["stackTrace"] = searchFailure.SourceException.StackTrace ?? string.Empty;
                }

                context.BotTelemetryClient.TrackEvent($"{context.ChatFlowId}StepLookupFailure", properties);
            }

            return context.RepeatSameStateJump();
        }

        async ValueTask<ChatFlowJump<T>> NextAsync(LookupCacheResult cacheResult)
        {
            var resultMessage = resultMessageFactory.Invoke(context, cacheResult.Value);
            await context.SendResultActivityAsync(resultMessage, cacheResult.Resources, token).ConfigureAwait(false);
            return mapFlowState.Invoke(context.FlowState, cacheResult.Value);
        }

        ValueTask<ChatFlowJump<T>> RepeatAsync()
            =>
            new(context.RepeatSameStateJump());
    }
}