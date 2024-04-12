using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<string>> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        ArgumentNullException.ThrowIfNull(resultMessageFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitText(chatFlow, optionFactory, resultMessageFactory, mapFlowState);
    }

    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<ValueStepOption<string>> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        ArgumentNullException.ThrowIfNull(resultMessageFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitText(chatFlow, CreateOption, resultMessageFactory, mapFlowState);

        ValueStepOption<string> CreateOption(IChatFlowContext<T> _)
            =>
            optionFactory.Invoke();
    }

    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<string>> optionFactory,
        Func<T, string, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitText(chatFlow, optionFactory, CreateDefaultResultMessage, mapFlowState);
    }

    private static ChatFlow<T> InnerAwaitText<T>(
        ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<string>> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
    {
        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitValueStepAsync(optionFactory, SuccessText, resultMessageFactory, mapFlowState, token);

        static Result<string, BotFlowFailure> SuccessText(string value)
            =>
            Result.Success(value);
    }
}