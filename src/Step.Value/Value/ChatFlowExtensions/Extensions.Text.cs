using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<string>> optionFactory,
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
        Func<ValueStepOption<string>> optionFactory,
        Func<IChatFlowContext<T>, string, string> resultMessageFactory,
        Func<T, string, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));

        _ = resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return InnerAwaitText(chatFlow, CreateOption, resultMessageFactory, mapFlowState);

        ValueStepOption<string> CreateOption(IChatFlowContext<T> _) => optionFactory.Invoke();
    }

    public static ChatFlow<T> AwaitText<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<string>> optionFactory,
        Func<T, string, T> mapFlowState)
        =>
        InnerAwaitText(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            CreateDefaultResultMessage,
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

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