using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

public static partial class CardChatFlowExtensions
{
    private static ValueTask<ChatFlowJump<T>> GetWebAppConfirmationResultOrRepeatAsync<T, TWebAppDataJson>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, TWebAppDataJson, Result<T, BotFlowFailure>> forwardFlowState,
        Func<T, ValueTask<ChatFlowJump<T>>> toNextAsync,
        CancellationToken cancellationToken)
        where TWebAppDataJson : notnull
    {
        if (context.IsNotTelegramChannel())
        {
            return context.RepeatSameStateValueTask();
        }

        var data = TelegramWebAppResponse.FromChannelData(context.Activity.ChannelData).Message?.WebAppData?.Data;
        if (string.IsNullOrWhiteSpace(data))
        {
            return context.RepeatSameStateValueTask();
        }

        var webAppData = JsonConvert.DeserializeObject<TWebAppDataJson>(data);
        if (webAppData is null)
        {
            return context.RepeatSameStateValueTask();
        }

        return forwardFlowState.Invoke(context, webAppData).FoldValueAsync(toNextAsync, RepeatAsync);

        async ValueTask<ChatFlowJump<T>> RepeatAsync(BotFlowFailure flowFailure)
        {
            if (string.IsNullOrEmpty(flowFailure.UserMessage) is false)
            {
                var invalidDateActivity = MessageFactory.Text(flowFailure.UserMessage);
                _ = await context.SendActivityAsync(invalidDateActivity, cancellationToken).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(flowFailure.LogMessage) is false || flowFailure.SourceException is not null)
            {
                context.Logger.LogError(flowFailure.SourceException, "{logMessage}", flowFailure.LogMessage);

                var properties = new Dictionary<string, string>
                {
                    ["flowId"] = context.ChatFlowId,
                    ["message"] = flowFailure.LogMessage
                };

                if (flowFailure.SourceException is not null)
                {
                    properties["errorMessage"] = flowFailure.SourceException.Message ?? string.Empty;
                    properties["errorType"] = flowFailure.SourceException.GetType().FullName ?? string.Empty;
                    properties["stackTrace"] = flowFailure.SourceException.StackTrace ?? string.Empty;
                }

                context.BotTelemetryClient.TrackEvent($"{context.ChatFlowId}StepAwaitConfirmation", properties);
            }

            return context.RepeatSameStateJump<T>();
        }
    }

    private static ValueTask<ChatFlowJump<T>> RepeatSameStateValueTask<T>(this IChatFlowContext<T> context)
        =>
        new(context.RepeatSameStateJump<T>());
}