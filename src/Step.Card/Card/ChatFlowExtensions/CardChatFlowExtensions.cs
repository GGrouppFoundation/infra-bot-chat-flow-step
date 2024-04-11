using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

public static partial class CardChatFlowExtensions
{
    private static async ValueTask<ChatFlowJump<T>> GetWebAppConfirmationResultOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, string, Result<T, BotFlowFailure>> forwardFlowState,
        Func<T, ValueTask<ChatFlowJump<T>>> toNextAsync,
        CancellationToken cancellationToken)
    {
        if (context.IsNotTelegramChannel())
        {
            return context.RepeatSameStateJump();
        }

        var data = TelegramWebAppResponse.FromChannelData(context.Activity.ChannelData).Message?.WebAppData?.Data;
        if (string.IsNullOrEmpty(data))
        {
            return context.RepeatSameStateJump();
        }

        var deletionTask = context.DeleteActivityAsync(context.Activity.Id, cancellationToken);
        var flowStateTask = forwardFlowState.Invoke(context, data).FoldValueAsync(toNextAsync, RepeatAsync).AsTask();

        await Task.WhenAll(deletionTask, flowStateTask).ConfigureAwait(false);
        return flowStateTask.Result;

        async ValueTask<ChatFlowJump<T>> RepeatAsync(BotFlowFailure flowFailure)
        {
            if (string.IsNullOrEmpty(flowFailure.UserMessage) is false)
            {
                var invalidDataActivity = context.CreateInvalidDataActivity(flowFailure.UserMessage);
                _ = await context.SendActivityAsync(invalidDataActivity, cancellationToken).ConfigureAwait(false);
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

            return context.RepeatSameStateJump();
        }
    }

    private static ValueTask<ChatFlowJump<T>> RepeatSameStateValueTask<T>(this IChatFlowContext<T> context)
        =>
        new(context.RepeatSameStateJump());
}