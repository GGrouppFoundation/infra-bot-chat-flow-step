using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    public static ChatFlow<T> AwaitDate<T>(
        this ChatFlow<T> chatFlow,
        Func<T, DateStepOption> optionFactory,
        Func<T, DateOnly, T> mapFlowState)
        =>
        InnerAwaitDate(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitDate<T>(
       this ChatFlow<T> chatFlow,
       Func<T, DateStepOption> optionFactory,
       Func<T, DateOnly, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.InnerAwaitDateAsync(optionFactory, mapFlowState, token));

    private static ValueTask<ChatFlowJump<T>> InnerAwaitDateAsync<T>(
        this IChatFlowContext<T> context,
        Func<T, DateStepOption> optionFactory,
        Func<T, DateOnly, T> mapFlowState,
        CancellationToken token)
        =>
        context.IsCardSupported()
        ? context.InnerAwaitDateAsync(optionFactory, CreateDateAdaptiveCardActivity, ParseDateFormAdaptiveCard, mapFlowState, token)
        : context.InnerAwaitDateAsync(optionFactory, CreateMessageActivity, ParseDateFromText, mapFlowState, token);

    private static async ValueTask<ChatFlowJump<T>> InnerAwaitDateAsync<T>(
        this IChatFlowContext<T> context,
        Func<T, DateStepOption> optionFactory,
        Func<ITurnContext, DateStepOption, IActivity> activityFactory,
        Func<ITurnContext, DateStepOption, Result<DateOnly, BotFlowFailure>> dateParser,
       Func<T, DateOnly, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context.FlowState);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        if (context.StepState is null)
        {
            var dateActivity = activityFactory.Invoke(context, option);
            await context.SendActivityAsync(dateActivity, cancellationToken).ConfigureAwait(false);

            return ChatFlowJump.Repeat<T>(new());
        }

        var dateResult = await dateParser.Invoke(context, option).MapFailureValueAsync(SendFailureActivityAsync).ConfigureAwait(false);
        return dateResult.MapSuccess(MapDate).Fold(ChatFlowJump.Next, context.RepeatSameStateJump<T>);

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

        T MapDate(DateOnly date)
            =>
            mapFlowState.Invoke(context.FlowState, date);
    }
}