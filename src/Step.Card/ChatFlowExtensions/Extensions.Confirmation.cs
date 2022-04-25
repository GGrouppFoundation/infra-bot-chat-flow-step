using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class CardChatFlowExtensions
{
    public static ChatFlow<T> AwaitConfirmation<T>(this ChatFlow<T> chatFlow, Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory)
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
        Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
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

        if (IsTextEqualTo(option.ConfirmButtonText))
        {
            return ToNext();
        }

        if (IsTextEqualTo(option.CancelButtonText))
        {
            return ToBreak();
        }

        return context.GetCardActionValueOrAbsent().Fold(CheckButtonId, ToRepeat);

        ChatFlowJump<T> CheckButtonId(Guid buttonId)
        {
            if (buttonId == cacheJson.ConfirmButtonGuid)
            {
                return ToNext();
            }

            if (buttonId == cacheJson.CancelButtonGuid)
            {
                return ToBreak();
            }

            return ToRepeat();
        }

        ChatFlowJump<T> ToNext()
            =>
            context.FlowState;

        ChatFlowJump<T> ToRepeat()
            =>
            context.RepeatSameStateJump<T>();

        ChatFlowJump<T> ToBreak()
            =>
            ChatFlowBreakState.From(option.CancelText);

        bool IsTextEqualTo(string buttonText)
            =>
            string.Equals(context?.Activity?.Text, buttonText, StringComparison.InvariantCultureIgnoreCase);
    }
}