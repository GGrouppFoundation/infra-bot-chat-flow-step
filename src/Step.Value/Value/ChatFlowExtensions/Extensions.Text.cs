using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
        =>
        InnerAwaitText(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<T, string, T> mapFlowState)
        =>
        InnerAwaitText(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            CreateDefaultResultMessage,
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitText<T>(
        ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
    {
        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitTextStepAsync(optionFactory, resultMessageFactory, mapFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> InvokeAwaitTextStepAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }
        
        var textJump = await context.GetTextOrRepeatAsync(option, cancellationToken).ConfigureAwait(false);
        return await textJump.MapValueAsync(SuccessAsync, ValueTask.FromResult, ValueTask.FromResult).ConfigureAwait(false);

        async ValueTask<T> SuccessAsync(string value)
        {
            await context.SendSuccessAsync(option, value, resultMessageFactory, cancellationToken);
            return mapFlowState.Invoke(context.FlowState, value);
        }
    }
}