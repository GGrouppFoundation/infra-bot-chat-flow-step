using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceStepContextExtensions
{
    internal static ValueTask<ChatFlowJump<T>> InternalGetChoiceJumpAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ChoiceStepOption> optionFactory,
        Func<T, ChoiceItem, T> mapFlowState,
        Func<T, T>? mapSkipButtonFlowState,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<ChatFlowJump<T>>(cancellationToken);
        }

        var stepOption = optionFactory.Invoke(context);
        if (stepOption.SkipStep)
        {
            return ValueTask.FromResult<ChatFlowJump<T>>(context.FlowState);
        }

        return context.InnerGetChoiceJumpAsync(stepOption, mapFlowState, mapSkipButtonFlowState, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<T>> InnerGetChoiceJumpAsync<T>(
        this IChatFlowContext<T> context,
        ChoiceStepOption stepOption,
        Func<T, ChoiceItem, T> mapFlowState,
        Func<T, T>? mapSkipButtonFlowState,
        CancellationToken cancellationToken)
    {
        var choiceItemJump = await context.InnerGetChoiceItemJumpAsync(stepOption, cancellationToken).ConfigureAwait(false);
        return choiceItemJump.Map(GetNextFlowState, Pipeline.Pipe, Pipeline.Pipe);

        T GetNextFlowState(ChoiceItem? item)
        {
            if (item is not null)
            {
                return mapFlowState.Invoke(context.FlowState, item);
            }

            if (mapSkipButtonFlowState is not null)
            {
                return mapSkipButtonFlowState.Invoke(context.FlowState);
            }

            return context.FlowState;
        }
    }

    private static ValueTask<ChatFlowJump<ChoiceItem?>> InnerGetChoiceItemJumpAsync<T>(
        this IChatFlowContext<T> context, ChoiceStepOption stepOption, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}