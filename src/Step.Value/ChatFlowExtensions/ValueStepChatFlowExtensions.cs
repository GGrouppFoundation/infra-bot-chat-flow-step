using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

public static partial class ValueStepChatFlowExtensions
{
    private static async ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync<T>(
        this IChatFlowStepContext context, ChatFlowStepFailure failure, CancellationToken token)
    {
        var uiMessage = failure.UIMessage;
        if (string.IsNullOrEmpty(uiMessage) is false)
        {
            var activity = MessageFactory.Text(uiMessage);
            await context.SendActivityAsync(activity, token).ConfigureAwait(false);
        }

        var logMessage = failure.LogMessage;
        if (string.IsNullOrEmpty(logMessage) is false)
        {
            context.Logger.LogError(logMessage);
        }

        return context.RepeatSameStateJump<T>(default);
    }

    private static async ValueTask<Result<string?, ChatFlowJump<T>>> GetTextOrRepeatJumpAsync<T>(
        this IChatFlowContext<SkipActivityOption> context,
        CancellationToken cancellationToken)
    {
        if (context.StepState is null)
        {
            var skupButtonId = Guid.NewGuid();

            var activity = context.CreateSkipActivity(context.FlowState, skupButtonId);
            await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return ChatFlowJump.Repeat<T>(skupButtonId);
        }

        return await context.GetTextOrFailure().MapFailureValueAsync(
            text => context.ToRepeatJumpAsync<T>(text, cancellationToken)).ConfigureAwait(false);
    }
}