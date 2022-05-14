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
        Func<IChatFlowContext<T>, DateStepOption> optionFactory,
        Func<T, DateOnly, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return chatFlow.ForwardValue(InnerAwaitDateAsync);

        ValueTask<ChatFlowJump<T>> InnerAwaitDateAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.IsCardSupported()
            ? context.InnerAwaitDateAsync(optionFactory, CreateDateAdaptiveCardActivity, ParseDateFormAdaptiveCard, mapFlowState, token)
            : context.InnerAwaitDateAsync(optionFactory, CreateMessageActivity, ParseDateFromText, mapFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> InnerAwaitDateAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, DateStepOption> optionFactory,
        Func<ITurnContext, DateStepOption, IActivity> activityFactory,
        Func<ITurnContext, DateStepOption, Result<DateOnly, BotFlowFailure>> dateParser,
        Func<T, DateOnly, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        if (context.StepState is DateCacheJson cacheJson)
        {
            return await dateParser.Invoke(context, option).FoldValueAsync(SuccessAsync, RepeatAsync).ConfigureAwait(false);
        }

        var dateActivity = activityFactory.Invoke(context, option);
        var resource = await context.SendActivityAsync(dateActivity, cancellationToken).ConfigureAwait(false);

        return ChatFlowJump.Repeat<T>(new DateCacheJson
        {
            Resource = context.IsMsteamsChannel() ? resource : null
        });

        async ValueTask<ChatFlowJump<T>> SuccessAsync(DateOnly date)
        {
            if (context.Activity.Value is not null)
            {
                var choosenText = context.EncodeTextWithStyle(date.ToString("dd.MM.yyyy"), BotTextStyle.Bold);
                var resultActivity = MessageFactory.Text($"{option.ResultText}: {choosenText}");
                await context.SendInsteadActivityAsync(cacheJson.Resource?.Id, resultActivity, cancellationToken).ConfigureAwait(false);
            }
            else if (cacheJson.Resource is not null)
            {
                var activity = MessageFactory.Text(option.Text);
                activity.Id = cacheJson.Resource.Id;

                await context.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            return mapFlowState.Invoke(context.FlowState, date);
        }

        async ValueTask<ChatFlowJump<T>> RepeatAsync(BotFlowFailure flowFailure)
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

            return context.RepeatSameStateJump<T>();
        }
    }

    private static Task SendInsteadActivityAsync(this ITurnContext context, string? activityId, IActivity activity, CancellationToken token)
    {
        return string.IsNullOrEmpty(activityId)
            ? SendActivityAsync()
            : Task.WhenAll(DeleteActivityAsync(), SendActivityAsync());

        Task SendActivityAsync()
            =>
            context.SendActivityAsync(activity, token);

        Task DeleteActivityAsync()
            =>
            context.DeleteActivityAsync(activityId, token);
    }
}