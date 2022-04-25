using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<T, string, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitTextStepAsync(optionFactory, mapFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> InvokeAwaitTextStepAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<T, string, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }
        
        var textJump = await context.GetTextOrRepeatAsync(option, cancellationToken).ConfigureAwait(false);
        return textJump.Map(MapValue, Pipeline.Pipe, Pipeline.Pipe);

        T MapValue(string value)
            =>
            mapFlowState.Invoke(context.FlowState, value);
    }
}