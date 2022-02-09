using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> GetTextOrSkip<T>(
        this ChatFlow<T> chatFlow,
        Func<T, SkipValueStepOption> optionFactory,
        Func<T, string?, T> mapFlowState)
        =>
        InnerGetTextOrSkip(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerGetTextOrSkip<T>(
        ChatFlow<T> chatFlow,
        Func<T, SkipValueStepOption> optionFactory,
        Func<T, string?, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => GetTextOrRepeatAsync(context, optionFactory, mapFlowState, token));

    private static async ValueTask<ChatFlowJump<T>> GetTextOrRepeatAsync<T>(
        IChatFlowContext<T> context,
        Func<T, SkipValueStepOption> optionFactory,
        Func<T, string?, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context.FlowState);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        var textResult = await context.GetTextOrRepeatJumpAsync<T>(option, cancellationToken).ConfigureAwait(false);
        return textResult.MapSuccess(MapText).Fold(ChatFlowJump.Next, Pipeline.Pipe);

        T MapText(string? value)
            =>
            mapFlowState.Invoke(context.FlowState, value);
    }
}