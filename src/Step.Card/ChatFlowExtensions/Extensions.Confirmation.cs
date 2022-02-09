using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CardChatFlowExtensions
{
    public static ChatFlow<T> AwaitConfirmation<T>(this ChatFlow<T> chatFlow, Func<T, ConfirmationCardOption> optionFactory)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));

        return chatFlow.ForwardValue(GetResultOrRepeatAsync);

        ValueTask<ChatFlowJump<T>> GetResultOrRepeatAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.GetConfirmationResultOrRepeatAsync(optionFactory, token);
    }

    private static async ValueTask<ChatFlowJump<T>> GetConfirmationResultOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        Func<T, ConfirmationCardOption> optionFactory,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context.FlowState);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        if (context.StepState is not ConfirmationCardCacheJson cacheJson)
        {
            cacheJson = new()
            {
                ConfirmButtonGuid = Guid.NewGuid(),
                CancelButtonGuid = Guid.NewGuid()
            };

            var activity = context.CreateConfirmationActivity(option, cacheJson);
            _ = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            return ChatFlowJump.Repeat<T>(cacheJson);
        }

        return context.GetCardActionValueOrAbsent().Fold(CheckButtonId, ToRepeat);

        ChatFlowJump<T> CheckButtonId(Guid buttonId)
        {
            if (buttonId == cacheJson.ConfirmButtonGuid)
            {
                return context.FlowState;
            }

            if (buttonId == cacheJson.CancelButtonGuid)
            {
                return ChatFlowBreakState.From(option.CancelText);
            }

            return ToRepeat();
        }

        ChatFlowJump<T> ToRepeat()
            =>
            context.RepeatSameStateJump<T>();
    }
}