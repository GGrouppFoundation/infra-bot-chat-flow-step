using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceStepContextExtensions
{
    internal static ValueTask<ChatFlowJump<T>> InternalGetChoiceJumpAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ChoiceStepOption> optionFactory,
        Func<T, ChoiceItem, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return new(context.FlowState);
        }

        if (context.StepState is not ChoiceStepStateJson stepState)
        {
            return context.InnerSendChoiceSetAsync(option, default, cancellationToken);
        }

        var selectedItem = context.GetSelectedItem(stepState);
        if (selectedItem is not null)
        {
            var resultActivity = context.CreateResultActivity(option, selectedItem);
            var nextState = mapFlowState.Invoke(context.FlowState, selectedItem);

            return context.InnerNextAsync(stepState, resultActivity, nextState, cancellationToken);
        }

        var request = context.GetChoiceSetRequest(stepState);
        if (request is null)
        {
            return new(context.RepeatSameStateJump());
        }

        return context.InnerSendChoiceSetAsync(option, request.Value, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<T>> InnerSendChoiceSetAsync<T>(
        this IChatFlowContext<T> context, ChoiceStepOption option, ChoiceSetRequest request, CancellationToken cancellationToken)
    {
        _ = await context.SetTypingStatusAsync(cancellationToken).ConfigureAwait(false);

        var choiceSetResult = await option.ChoiceSetFactory.Invoke(request, cancellationToken).ConfigureAwait(false);
        return await choiceSetResult.Fold(InnerRepeatWithChoiceSetAsync, InnerRepeatWithFailureAsync).ConfigureAwait(false);

        async Task<ChatFlowJump<T>> InnerRepeatWithChoiceSetAsync(ChoiceSetOption choiceSet)
        {
            var activities = context.CreateChoiceActivitiesAsync(choiceSet);
            var resources = await context.SendActivitiesAsync(activities, cancellationToken).ConfigureAwait(false);

            return context.RepeatWithNewStepState(resources, request, choiceSet);
        }

        async Task<ChatFlowJump<T>> InnerRepeatWithFailureAsync(BotFlowFailure failure)
        {
            await context.SendFlowFailureAsync(failure, cancellationToken).ConfigureAwait(false);
            context.LogFlowFailure(failure);

            return context.RepeatSameStateJump();
        }
    }

    private static async ValueTask<ChatFlowJump<T>> InnerNextAsync<T>(
        this IChatFlowContext<T> context, ChoiceStepStateJson stepState, IActivity resultActivity, T nextState, CancellationToken cancellationToken)
    {
        await context.InstallResultActivityAsync(resultActivity, stepState.Resources, cancellationToken).ConfigureAwait(false);
        return nextState;
    }
}