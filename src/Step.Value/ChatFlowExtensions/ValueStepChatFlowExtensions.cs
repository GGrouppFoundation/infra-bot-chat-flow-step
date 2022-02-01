using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

public static partial class ValueStepChatFlowExtensions
{
    private static async ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync<T>(
        this IChatFlowStepContext context, BotFlowFailure failure, CancellationToken token)
    {
        var userMessage = failure.UserMessage;
        if (string.IsNullOrEmpty(userMessage) is false)
        {
            var activity = MessageFactory.Text(userMessage);
            await context.SendActivityAsync(activity, token).ConfigureAwait(false);
        }

        var logMessage = failure.LogMessage;
        if (string.IsNullOrEmpty(logMessage) is false)
        {
            context.Logger.LogError("{logMessage}", logMessage);
        }

        return context.RepeatSameStateJump<T>(default);
    }

    private static async ValueTask<Result<string?, ChatFlowJump<T>>> GetTextOrRepeatJumpAsync<T>(
        this IChatFlowContext<SkipActivityOption> context, CancellationToken cancellationToken)
    {
        if (context.StepState is null)
        {
            var skipButtonId = Guid.NewGuid();

            var activity = context.CreateSkipActivity(context.FlowState, skipButtonId);
            await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return ChatFlowJump.Repeat<T>(skipButtonId);
        }

        return await context.GetTextOrFailure(context.FlowState).MapFailureValueAsync(ToRepeatJumpAsync).ConfigureAwait(false);

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T>(failure, cancellationToken);
    }
}