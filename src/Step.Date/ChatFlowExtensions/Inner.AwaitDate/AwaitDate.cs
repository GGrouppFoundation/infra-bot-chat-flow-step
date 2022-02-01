using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    public static ChatFlow<TNext> AwaitDate<T, TNext>(
        this ChatFlow<T> chatFlow,
        Func<T, AwaitDateOption> optionFactory,
        Func<T, DateOnly, TNext> mapFlowState)
        =>
        InnerAwaitDate(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerAwaitDate<T, TNext>(
       this ChatFlow<T> chatFlow,
       Func<T, AwaitDateOption> optionFactory,
       Func<T, DateOnly, TNext> mapFlowState)
        =>
        chatFlow.ForwardValue(optionFactory, InnerAwaitDateAsync, mapFlowState);

    private static ValueTask<ChatFlowJump<DateOnly>> InnerAwaitDateAsync(
        IChatFlowContext<AwaitDateOption> context, CancellationToken cancellationToken)
        =>
        context.IsCardSupported()
        ? context.InnerAwaitDateAsync(CreateDateAdaptiveCardActivity, ParseDateFormAdaptiveCard, cancellationToken)
        : context.InnerAwaitDateAsync(CreateMessageActivity, ParseDateFromText, cancellationToken);

    private static async ValueTask<ChatFlowJump<DateOnly>> InnerAwaitDateAsync(
        this IChatFlowContext<AwaitDateOption> context,
        Func<IChatFlowContext<AwaitDateOption>, IActivity> activityFactory,
        Func<IChatFlowContext<AwaitDateOption>, Result<DateOnly, BotFlowFailure>> dateParser,
        CancellationToken cancellationToken)
    {
        var option = context.FlowState;

        if (context.StepState is null)
        {
            var dateActivity = activityFactory.Invoke(context);
            await context.SendActivityAsync(dateActivity, cancellationToken).ConfigureAwait(false);

            return ChatFlowJump.Repeat<DateOnly>(new());
        }

        var dateResult = await dateParser.Invoke(context).MapFailureValueAsync(SendFailureActivityAsync).ConfigureAwait(false);
        return dateResult.Fold(NextDateJump, context.RepeatSameStateJump<DateOnly>);

        async ValueTask<Unit> SendFailureActivityAsync(BotFlowFailure flowFailure)
        {
            if (string.IsNullOrEmpty(flowFailure.UserMessage) is false)
            {
                var invalidDateActivity = MessageFactory.Text(option.InvalidDateText);
                await context.SendActivityAsync(invalidDateActivity, cancellationToken).ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(flowFailure.LogMessage) is false)
            {
                context.Logger.LogError("{logMessage}", flowFailure.LogMessage);
            }

            return default;
        }
    }
}