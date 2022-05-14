using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));

        _ = valueParser ?? throw new ArgumentNullException(nameof(valueParser));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitValueStepAsync(optionFactory, valueParser, mapFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> InvokeAwaitValueStepAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }
        
        var textJump = await context.GetTextOrRepeatAsync(option, cancellationToken).ConfigureAwait(false);
        return await textJump.ForwardValueAsync(ParseAsync).ConfigureAwait(false);

        ValueTask<ChatFlowJump<T>> ParseAsync(string text)
        {
            return valueParser.Invoke(text).FoldValueAsync(ToNextAsync, ToRepeatJumpAsync);;

            async ValueTask<ChatFlowJump<T>> ToNextAsync(TValue value)
            {
                await context.SendSuccessAsync(option, text, cancellationToken);
                return mapFlowState.Invoke(context.FlowState, value);
            }
        }

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T>(failure, cancellationToken);
    }
}